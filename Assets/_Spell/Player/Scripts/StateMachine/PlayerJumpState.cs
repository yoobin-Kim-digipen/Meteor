using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private Player _player;
    public PlayerJumpState(PlayerStateMachine context, PlayerStateFactory factory) : base(context, factory)
    {
        _player = _ctx.playerMovement;
    }

    public override void EnterState()
    {
        Debug.Log("Enter Jump State");

        // 점프 큐 클리어
        _player.jumpQueued = false;
        _player.jumpReleaseQueued = false;

        // (물리식) 목표 높이 h에 도달할 초기 속도: v = sqrt(2gh)
        float vy = Mathf.Sqrt(Mathf.Max(0f, -2f * Physics.gravity.y * _player.jumpHeight));

        Vector3 v = _player.Rigidbody.linearVelocity;

        // 수직 성분 초기화(일관된 높이)
        v.y = 0f;
        _player.Rigidbody.linearVelocity = v;

        // 위로만 즉시 속도 부여(질량 무관)
        _player.Rigidbody.AddForce(Vector3.up * vy, ForceMode.VelocityChange);

        // 점프 시작 시간 기록
        _player.jumpStartTime = Time.time;
        _player.ungroundedUntil = Time.time + _player.jumpUngroundGrace; // 착지 무시 시간
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        // 점프 컷 처리 (공중에서 스페이스 뗐을 때)
        if (_player.jumpCut && _player.jumpReleaseQueued &&
            _player.Rigidbody.linearVelocity.y > 0f &&
            (Time.time - _player.jumpStartTime) >= _player.minJumpCutDelay)
        {
            _player.jumpReleaseQueued = false;

            Vector3 v = _player.Rigidbody.linearVelocity;
            v.y *= Mathf.Clamp01(_player.jumpCutMultiplier);
            _player.Rigidbody.linearVelocity = v;

            Debug.Log("Jump Cut Applied!");
        }

        HandleJumpMovement();
    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchStates()
    {
        // 점프 최고점 지나서 하강 중이면 Fall로
        if (_player.Rigidbody.linearVelocity.y < 0f)
        {
            _ctx.SwitchState(_factory.Fall());
            return;
        }
    }

    private void HandleJumpMovement()
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
            float proj = Vector3.Dot(planar, wishDir);
            float addNeeded = targetSpeed - proj;

            if (addNeeded > 0f)
            {
                float add = Mathf.Min(_player.airAcceleration * dt, addNeeded);
                planar += wishDir * add;
            }
            else if (addNeeded < 0f && _player.airDeceleration > 0f)
            {
                float reduce = Mathf.Min(_player.airDeceleration * dt, -addNeeded);
                planar += wishDir * (-reduce);
            }
        }
        else
        {
            if (_player.airDeceleration > 0f)
                planar = Vector3.MoveTowards(planar, Vector3.zero, _player.airDeceleration * dt);
        }

        v.x = planar.x;
        v.z = planar.z;
        _player.Rigidbody.linearVelocity = v;

        // 회전 처리
        if (_player.wantRotate)
        {
            _player.faceCameraYaw();
        }
    }
}