public class PlayerStateFactory
{
    private PlayerStateMachine _context;

    private PlayerBaseState _groundedState;
    private PlayerBaseState _jumpState;
    private PlayerBaseState _fallState;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;

        _groundedState = new PlayerGroundedState(_context, this);
        _jumpState = new PlayerJumpState(_context, this);
        _fallState = new PlayerFallState(_context, this);
    }

    public PlayerBaseState Grounded() => _groundedState;
    public PlayerBaseState Jump() => _jumpState;
    public PlayerBaseState Fall() => _fallState;
}