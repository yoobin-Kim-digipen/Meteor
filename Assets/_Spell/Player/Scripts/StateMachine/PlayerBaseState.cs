public abstract class PlayerBaseState
{
    protected PlayerStateMachine _ctx; // Context (상태 머신의 주인)
    protected PlayerStateFactory _factory; // 상태들을 생성하는 팩토리

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