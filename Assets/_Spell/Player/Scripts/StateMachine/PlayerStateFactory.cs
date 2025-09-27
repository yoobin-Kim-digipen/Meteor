public class PlayerStateFactory
{
    private PlayerStateMachine _context;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
    }

    // 각 상태를 생성하는 메소드들
    public PlayerBaseState Grounded() { return new PlayerGroundedState(_context, this); }
    public PlayerBaseState Jump() { return new PlayerJumpState(_context, this); }
    public PlayerBaseState Fall() { return new PlayerFallState(_context, this); }
    // 나중에 Aim, Attack 등 다른 상태들도 여기에 추가
}