using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGroundedState : PlayerBaseState
{
    private Player _player;
    private float _groundedGraceTimer; // <--- 유예 시간을 셀 타이머 변수 추가
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory factory) : base(context, factory)
    {
        _player = _ctx.playerMovement; // Player 컴포넌트 참조
    }

    public override void EnterState()
    {
        //Debug.Log("Enter Grounded State");
        _player.jumpReleaseQueued = false;
        _groundedGraceTimer = _player.groundedGracePeriod;
    }

    public override void UpdateState()
    {
        // 매 프레임 상태 전환 조건을 확인
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        // 공격 중일 때와 아닐 때의 로직을 분리
        if (_player.IsAttacking)
        {
            // 1. 공격 관리자에게 조준과 발사를 모두 위임
            if (_player.attackManager != null)
            {
                _player.attackManager.HandleAimAndAttack();
            }

            // 2. 공격 중에도 이동은 가능하도록 처리
            // 단, 회전은 AttackManager가 하므로 여기서는 회전 로직을 제외
            HandleGroundedMovement(applyRotation: false);
        }
        else
        {
            // 3. 공격 중이 아닐 때는 평소처럼 이동과 회전을 모두 처리
            if (_player.attackManager != null)
            {
                _player.attackManager.OnStopAttack();
            }

            HandleGroundedMovement(applyRotation: true);
        }
    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchStates()
    {
        // 점프 조건은 최우선으로 확인
        if (_player.jumpQueued && _player.isGrounded)
        {
            _ctx.SwitchState(_factory.Jump());
            return;
        }

        if (_player.isGrounded)
        {
            // 땅에 붙어있다면, 타이머를 계속 최대로 유지
            _groundedGraceTimer = _player.groundedGracePeriod;
        }
        else
        {
            // 땅에서 떨어졌다면, 타이머를 감소시키기 시작
            _groundedGraceTimer -= Time.deltaTime;

            // 타이머가 0 이하로 떨어져야만 '진짜 추락'으로 간주하고 Fall 상태로 전환
            if (_groundedGraceTimer <= 0f)
            {
                _ctx.SwitchState(_factory.Fall());
            }
        }
    }
    private void HandleGroundedMovement(bool applyRotation)
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
        if (applyRotation && _player.wantRotate)
        {
            _player.faceCameraYaw(); // 평소의 회전 로직
        }
    }
}