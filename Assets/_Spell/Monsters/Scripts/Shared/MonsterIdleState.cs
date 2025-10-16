using UnityEngine;

public class MonsterIdleState : MonsterBaseState
{
    public MonsterIdleState(MonsterStateMachine context, MonsterStateFactory factory) : base(context, factory) { }

    public override void EnterState()
    {
        _monster.agent.isStopped = true;
    }

    public override void UpdateState()
    {
        if (_monster.target != null)
        {
            _ctx.SwitchState(_factory.Chase());
            return;
        }
    }

    public override void ExitState() { }
}