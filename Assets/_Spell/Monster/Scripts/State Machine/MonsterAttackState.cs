using UnityEngine;

public class MonsterAttackState : MonsterBaseState
{
    private float _attackCooldownTimer;

    public MonsterAttackState(MonsterStateMachine context, MonsterStateFactory factory) : base(context, factory) { }

    public override void EnterState()
    {
        //Debug.Log("몬스터: 공격 시작!");
        _attackCooldownTimer = 0f; // 쿨타임 초기화
    }

    public override void UpdateState()
    {
        // 플레이어가 공격 범위를 벗어났는지 확인
        if (!_monster.IsPlayerInAttackRange())
        {
            _ctx.SwitchState(_factory.Chase()); // 다시 추적 상태로 전환 요청
            return;
        }

        // 쿨타임 계산
        _attackCooldownTimer -= Time.deltaTime;
        if (_attackCooldownTimer <= 0)
        {
            PerformAttack();
            // 쿨타임을 몬스터 데이터에서 가져오도록 확장할 수 있습니다.
            // _attackCooldownTimer = _monster.monsterData.attackCooldown; 
            _attackCooldownTimer = 2f; // 임시로 2초 쿨타임 설정
        }
    }

    public override void ExitState()
    {
        //_monster.Animator.ResetTrigger("Attack"); // 공격 애니메이션 트리거 초기화
    }

    private void PerformAttack()
    {
        //Debug.Log("몬스터가 플레이어를 공격합니다!");
        //_monster.Animator.SetTrigger("Attack"); // 공격 애니메이션 재생

        // ex) 여기에 실제 데미지를 주는 로직을 구현
        // 예: 플레이어의 TakeDamage 함수 호출
        // _monster.target.GetComponent<PlayerHealth>().TakeDamage(_monster.monsterData.damage);
    }
}