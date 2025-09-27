using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    private Player _player;

    public PlayerFallState(PlayerStateMachine context, PlayerStateFactory factory) : base(context, factory)
    {
        _player = _ctx.playerMovement;
    }

    public override void EnterState()
    {
        Debug.Log("Enter Fall State");
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        HandleFallMovement();
    }

    public override void ExitState()
    {
        // 착지 직전 처리 (랜딩 애니메이션, 사운드 등)
    }

    public override void CheckSwitchStates()
    {
        // 다시 바닥에 닿으면 Grounded로
        if (_player.isGrounded)
        {
            _ctx.SwitchState(_factory.Grounded());
            return;
        }
    }

    private void HandleFallMovement()
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

        // 낙하 중 중력 강화
        if (!_player.isGrounded && _player.Rigidbody.linearVelocity.y < 0f)
        {
            _player.Rigidbody.AddForce(Physics.gravity * (_player.fallMultiplier - 1f), ForceMode.Acceleration);
        }

        // 회전 처리
        if (_player.wantRotate)
        {
            _player.faceCameraYaw();
        }
    }
}