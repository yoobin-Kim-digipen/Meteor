public abstract class PlayerBaseState
{
    protected PlayerStateMachine _ctx; // Context (���� �ӽ��� ����)
    protected PlayerStateFactory _factory; // ���µ��� �����ϴ� ���丮

    public PlayerBaseState(PlayerStateMachine context, PlayerStateFactory factory)
    {
        _ctx = context;
        _factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();
}