public abstract class MonsterBaseState
{
    protected MonsterStateMachine _ctx;
    protected MonsterStateFactory _factory;
    protected MonsterFSM _monster;

    public MonsterBaseState(MonsterStateMachine context, MonsterStateFactory factory)
    {
        _ctx = context;
        _factory = factory;
        _monster = context.MonsterFSM;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}