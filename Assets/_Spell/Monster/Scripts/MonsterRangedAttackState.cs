using UnityEngine;

public class MonsterRangedAttackState : MonsterBaseState
{
    private RangeMonsterData _rangedData;
    private float _attackCooldownTimer;

    public MonsterRangedAttackState(MonsterStateMachine context, MonsterStateFactory factory) : base(context, factory)
    {
        // FSM이 가지고 있는 데이터를 RangedMonsterData 타입으로 가져옴
        _rangedData = _monster.monsterData as RangeMonsterData;
    }

    public override void EnterState()
    {
        _attackCooldownTimer = _rangedData.attackCooldown * 0.5f; // 처음엔 더 빨리 쏘도록
        _monster.agent.isStopped = false; // 거리 조절을 위해 계속 움직일 수 있게 함
    }

    public override void UpdateState()
    {
        // 1. 공격할 대상이 없으면 추적 상태로 전환
        if (_monster.target == null)
        {
            _ctx.SwitchState(_factory.Chase());
            return;
        }

        // 2. 플레이어를 계속 바라보도록 함
        Vector3 lookDirection = (_monster.target.position - _monster.transform.position);
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            _monster.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // 3. 거리 유지 및 공격
        HandleBehavior();
    }

    private void HandleBehavior()
    {
        float distance = Vector3.Distance(_monster.transform.position, _monster.target.position);

        // 너무 멀어지면 다시 추적 상태로
        if (distance > _rangedData.attackRange)
        {
            _ctx.SwitchState(_factory.Chase());
            return;
        }

        // 너무 가까워지면 뒤로 물러남
        if (distance < _rangedData.tooCloseDistance)
        {
            Vector3 awayFromTarget = (_monster.transform.position - _monster.target.position).normalized;
            _monster.agent.SetDestination(_monster.transform.position + awayFromTarget);
        }
        else // 적정 거리면 공격
        {
            _monster.agent.ResetPath(); // 제자리에 멈춤

            // 쿨타임마다 공격 실행
            _attackCooldownTimer -= Time.deltaTime;
            if (_attackCooldownTimer <= 0)
            {
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        // 1. 쿨타임 재설정하고 사용할 스킬 데이터 가져오기
            _attackCooldownTimer = _rangedData.attackCooldown;
        if (_rangedData.skills == null || _rangedData.skills.Count == 0) return;
        SkillData skillToUse = _rangedData.skills[0];

        // 2. 스킬 발사 시작 위치 계산 (몬스터 몸통 앞)
        Vector3 spawnPos = _monster.transform.position + _monster.transform.rotation * skillToUse.spawnOffset;

        Vector3 finalTargetPoint = _monster.target.position;

        // 3. 몬스터가 타겟의 콜라이더 정보를 가지고 있는지 확인
        if (_monster.targetCollider != null)
        {
            // 콜라이더가 있으면, 그것의 정확한 중심점을 조준점으로 사용
            Vector3 targetCenter = _monster.targetCollider.bounds.center;

            targetCenter.y -= 1.5f; // 귀찮아서 일단 높이 좀 낮춤
            finalTargetPoint = targetCenter;
        }

        // 4. 최종 목표 지점을 정확히 바라보는 발사 각도를 계산
        Quaternion spawnRotation = Quaternion.LookRotation((finalTargetPoint - spawnPos).normalized);

        // 5. 오브젝트 풀에서 스킬 오브젝트를 가져옴
        GameObject skillObj = ObjectPooler.Instance.GetFromPool(skillToUse.skillName, spawnPos, spawnRotation);

        if (skillObj != null)
        {
            // 6. 스킬 컴포넌트를 찾아서 Activate 함수 호출. "내가 이 스킬 쏜다!"고 알려줌
            Skill skill = skillObj.GetComponent<Skill>();
            if (skill != null)
            {
                skill.Activate(_monster.gameObject, skillToUse);
            }
        }
    }
    public override void ExitState() { }
}