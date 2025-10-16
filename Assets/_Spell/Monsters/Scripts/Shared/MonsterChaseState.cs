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
        // 만약 내가 '소굴 소속'인데, 플레이어가 영역 밖에 있거나 타겟이 없으면 Idle로 돌아감
        if (_monster.aiType == AIType.LairGuardian)
        {
            if (_monster.Lair == null || !_monster.Lair.IsPlayerInLair || _monster.target == null)
            {
                // 위 조건 중 하나라도 만족하면 (소굴 정보가 없거나, 플레이어가 없거나, 타겟 지정이 안됐거나) Idle로 돌아감
                _ctx.SwitchState(_factory.Idle());
                return;
            }
        }

        // 목표(플레이어)가 있는지 확인하고 목적지 설정
        if (_monster.target != null)
        {
            _monster.agent.SetDestination(_monster.target.position);

            // 공격 범위에 들어왔는지 확인
            if (_monster.IsPlayerInAttackRange())
            {
                _ctx.SwitchState(_factory.Attack()); // 공격 상태로 전환 요청
            }
        }

        else // 타겟이 없으면 (예: 야생 몬스터 스폰 직후 타겟 설정 실패)
        {
            _ctx.SwitchState(_factory.Idle());
        }
    }

    public override void ExitState()
    {
        //_monster.Animator.SetBool("IsRunning", false); // 달리기 애니메이션 중지
        _monster.agent.isStopped = true; // NavMeshAgent 이동 중지
    }
}