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
        // ���� ���� ó�� (���� �ִϸ��̼�, ���� ��)
    }

    public override void CheckSwitchStates()
    {
        // �ٽ� �ٴڿ� ������ Grounded��
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

        // ��麤�� ����� (x��� z�ุ ���)
        Vector3 planar = new Vector3(v.x, 0f, v.z);

        // clamp������� �Է°� ����ȭ
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

        // ���� �� �߷� ��ȭ
        if (!_player.isGrounded && _player.Rigidbody.linearVelocity.y < 0f)
        {
            _player.Rigidbody.AddForce(Physics.gravity * (_player.fallMultiplier - 1f), ForceMode.Acceleration);
        }

        // ȸ�� ó��
        if (_player.wantRotate)
        {
            _player.faceCameraYaw();
        }
    }
}