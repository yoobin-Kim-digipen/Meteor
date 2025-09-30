using UnityEngine;

public class MonsterChaseState : MonsterBaseState
{
    public MonsterChaseState(MonsterStateMachine context, MonsterStateFactory factory) : base(context, factory) { }

    public override void EnterState()
    {
        //Debug.Log("몬스터: 추적 시작!");
        //_monster.Animator.SetBool("IsRunning", true); // 달리기 애니메이션 재생
        _monster.agent.isStopped = false; // NavMeshAgent 이동 시작
    }

    public override void UpdateState()
    {
        // 목표(플레이어)가 있는지 확인하고 목적지 설정
        if (_monster.target != null)
        {
            _monster.agent.SetDestination(_monster.target.position);
        }

        // 공격 범위에 들어왔는지 확인
        if (_monster.IsPlayerInAttackRange())
        {
            _ctx.SwitchState(_factory.Attack()); // 공격 상태로 전환 요청
        }
    }

    public override void ExitState()
    {
        //_monster.Animator.SetBool("IsRunning", false); // 달리기 애니메이션 중지
        _monster.agent.isStopped = true; // NavMeshAgent 이동 중지
    }
}