using UnityEngine;

public class MonsterReturnState : MonsterBaseState
{
    public MonsterReturnState(MonsterStateMachine context, MonsterStateFactory factory) : base(context, factory) { }

    public override void EnterState()
    {
        // 원래 위치로 이동 시작
        _monster.agent.isStopped = false;
        _monster.agent.SetDestination(_monster.GetInitialPosition());
    }

    public override void UpdateState()
    {
        // 원래 위치에 거의 도착했는지 확인
        if (!_monster.agent.pathPending && _monster.agent.remainingDistance < 0.5f)
        {
            // 도착했으면 '대기' 상태로 전환
            _ctx.SwitchState(_factory.Idle());
        }

        if (_monster.IsInLairCombat)
        {
            _ctx.SwitchState(_factory.Chase());
        }
    }

    public override void ExitState() { }
}