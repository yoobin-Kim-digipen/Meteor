using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGroundedState : PlayerBaseState
{
    private Player _player;
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory factory) : base(context, factory)
    {
        _player = _ctx.playerMovement; // Player 컴포넌트 참조
    }

    public override void EnterState()
    {
        Debug.Log("Enter Grounded State");
        _player.jumpReleaseQueued = false;
    }

    public override void UpdateState()
    {
        // 매 프레임 상태 전환 조건을 확인
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        HandleGroundedMovement();
    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchStates()
    {
        // 점프 큐가 있고, 실제로 grounded라면 점프 상태로
        if (_player.jumpQueued && _player.isGrounded)
        {
            _ctx.SwitchState(_factory.Jump());
            return;
        }

        // 공중 상태로 전환 조건: grounded가 아니고, 아래로 떨어지고 있을 때
        if (!_player.isGrounded && _player.Rigidbody.linearVelocity.y < 0f)
        {
            _ctx.SwitchState(_factory.Fall());
            return;
        }
    }
    private void HandleGroundedMovement()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 v = _player.Rigidbody.linearVelocity;

        // 평면벡터 만들기 (x축과 z축만 사용)
        Vector3 planar = new Vector3(v.x, 0f, v.z);

        // clamp사용으로 입력값 정규화
        float targetSpeed = _player.moveSpeed * Mathf.Clamp01(_player.moveDir.magnitude);
        Vector3 wishDir = (_player.moveDir.sqrMagnitude > 1e-6f) ? _player.moveDir.normalized : Vector3.zero;

        if (wishDir != Vector3.zero)
        {
            // 지상: 입력 방향으로 '가속만' 더해줌(감속은 별도)
            // 현재 속도의 "wishDir 방향 성분" (투영값)
            // → 예: wishDir=전방, planar=전방 3 + 옆 1 → curAlong=3
            float curAlong = Vector3.Dot(planar, wishDir); // 입력 방향 성분 속도

            // → curAlong < target → 양수 (가속 필요), > target → 음수 (이미 빠름)
            float addNeeded = targetSpeed - curAlong;

            if (addNeeded > 0f)
            {
                // 부드러운 가속을 위한 Min사용 급가속 방지
                float add = Mathf.Min(_player.acceleration * dt, addNeeded);
                planar += wishDir * add;
            }
            else
            {
                // 반대 방향 입력: 적당히 감속
                float reduce = Mathf.Min(_player.deceleration * dt, -addNeeded);
                planar += wishDir * (-reduce);
            }

            // 최고 속도 살짝 클램프(넘치면 조금만 깎기)
            // 가속 후 magnitude가 targetSpeed 초과하면(옆 입력 등으로), 초과분만 deceleration*dt만큼 깎음.
            // normalized * (magnitude - cut): 방향 유지하면서 길이만 줄임 → "슬라이드" 방지
            if (planar.magnitude > targetSpeed)
            {
                float over = planar.magnitude - targetSpeed;
                float cut = Mathf.Min(_player.deceleration * dt, over);
                planar = planar.normalized * (planar.magnitude - cut);
            }
        }
        else
        {
            // 입력 없으면 서서히 감속
            // MoveTowards는 클램프 내장 → over-shoot 없음
            planar = Vector3.MoveTowards(planar, Vector3.zero, _player.deceleration * dt);
        }

        v.x = planar.x;
        v.z = planar.z;
        _player.Rigidbody.linearVelocity = v;

        // 회전 처리 (카메라 방향)
        if (_player.wantRotate)
        {
            _player.faceCameraYaw(); // 이건 Player.cs에 남겨둠
        }
    }
}