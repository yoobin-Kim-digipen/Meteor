public class PlayerStateFactory
{
    private PlayerStateMachine _context;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }

    // �� ���¸� �����ϴ� �޼ҵ��
    public PlayerBaseState Grounded() { return new PlayerGroundedState(_context, this); }
    public PlayerBaseState Jump() { return new PlayerJumpState(_context, this); }
    public PlayerBaseState Fall() { return new PlayerFallState(_context, this); }
    // ���߿� Aim, Attack �� �ٸ� ���µ鵵 ���⿡ �߰�
}