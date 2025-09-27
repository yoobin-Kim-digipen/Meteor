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

        // ���� ť Ŭ����
        _player.jumpQueued = false;
        _player.jumpReleaseQueued = false;

        // (������) ��ǥ ���� h�� ������ �ʱ� �ӵ�: v = sqrt(2gh)
        float vy = Mathf.Sqrt(Mathf.Max(0f, -2f * Physics.gravity.y * _player.jumpHeight));

        Vector3 v = _player.Rigidbody.linearVelocity;

        // ���� ���� �ʱ�ȭ(�ϰ��� ����)
        v.y = 0f;
        _player.Rigidbody.linearVelocity = v;

        // ���θ� ��� �ӵ� �ο�(���� ����)
        _player.Rigidbody.AddForce(Vector3.up * vy, ForceMode.VelocityChange);

        // ���� ���� �ð� ���
        _player.jumpStartTime = Time.time;
        _player.ungroundedUntil = Time.time + _player.jumpUngroundGrace; // ���� ���� �ð�
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        // ���� �� ó�� (���߿��� �����̽� ���� ��)
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
        // ���� �ְ��� ������ �ϰ� ���̸� Fall��
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

        // ȸ�� ó��
        if (_player.wantRotate)
        {
            _player.faceCameraYaw();
        }
    }
}