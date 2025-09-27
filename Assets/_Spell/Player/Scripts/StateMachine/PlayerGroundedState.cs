using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGroundedState : PlayerBaseState
{
    private Player _player;
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory factory) : base(context, factory)
    {
        _player = _ctx.playerMovement; // Player ������Ʈ ����
    }

    public override void EnterState()
    {
        Debug.Log("Enter Grounded State");
        _player.jumpReleaseQueued = false;
    }

    public override void UpdateState()
    {
        // �� ������ ���� ��ȯ ������ Ȯ��
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
        // ���� ť�� �ְ�, ������ grounded��� ���� ���·�
        if (_player.jumpQueued && _player.isGrounded)
        {
            _ctx.SwitchState(_factory.Jump());
            return;
        }

        // ���� ���·� ��ȯ ����: grounded�� �ƴϰ�, �Ʒ��� �������� ���� ��
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

        // ��麤�� ����� (x��� z�ุ ���)
        Vector3 planar = new Vector3(v.x, 0f, v.z);

        // clamp������� �Է°� ����ȭ
        float targetSpeed = _player.moveSpeed * Mathf.Clamp01(_player.moveDir.magnitude);
        Vector3 wishDir = (_player.moveDir.sqrMagnitude > 1e-6f) ? _player.moveDir.normalized : Vector3.zero;

        if (wishDir != Vector3.zero)
        {
            // ����: �Է� �������� '���Ӹ�' ������(������ ����)
            // ���� �ӵ��� "wishDir ���� ����" (������)
            // �� ��: wishDir=����, planar=���� 3 + �� 1 �� curAlong=3
            float curAlong = Vector3.Dot(planar, wishDir); // �Է� ���� ���� �ӵ�

            // �� curAlong < target �� ��� (���� �ʿ�), > target �� ���� (�̹� ����)
            float addNeeded = targetSpeed - curAlong;

            if (addNeeded > 0f)
            {
                // �ε巯�� ������ ���� Min��� �ް��� ����
                float add = Mathf.Min(_player.acceleration * dt, addNeeded);
                planar += wishDir * add;
            }
            else
            {
                // �ݴ� ���� �Է�: ������ ����
                float reduce = Mathf.Min(_player.deceleration * dt, -addNeeded);
                planar += wishDir * (-reduce);
            }

            // �ְ� �ӵ� ��¦ Ŭ����(��ġ�� ���ݸ� ���)
            // ���� �� magnitude�� targetSpeed �ʰ��ϸ�(�� �Է� ������), �ʰ��и� deceleration*dt��ŭ ����.
            // normalized * (magnitude - cut): ���� �����ϸ鼭 ���̸� ���� �� "�����̵�" ����
            if (planar.magnitude > targetSpeed)
            {
                float over = planar.magnitude - targetSpeed;
                float cut = Mathf.Min(_player.deceleration * dt, over);
                planar = planar.normalized * (planar.magnitude - cut);
            }
        }
        else
        {
            // �Է� ������ ������ ����
            // MoveTowards�� Ŭ���� ���� �� over-shoot ����
            planar = Vector3.MoveTowards(planar, Vector3.zero, _player.deceleration * dt);
        }

        v.x = planar.x;
        v.z = planar.z;
        _player.Rigidbody.linearVelocity = v;

        // ȸ�� ó�� (ī�޶� ����)
        if (_player.wantRotate)
        {
            _player.faceCameraYaw(); // �̰� Player.cs�� ���ܵ�
        }
    }
}