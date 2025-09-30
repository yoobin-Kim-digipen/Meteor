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
        _attackCooldownTimer = _rangedData.attackCooldown;
        // _monster.Animator.SetTrigger("Attack");

        // 몬스터가 사용할 스킬이 등록되어 있는지 확인
        if (_rangedData.skills == null || _rangedData.skills.Count == 0) return;

        // 이 몬스터의 첫 번째 스킬을 사용
        SkillData skillToUse = _rangedData.skills[0];

        // 스폰 위치 계산
        Transform spawnPoint = _monster.transform.Find("SpawnPoint");
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : _monster.transform.position + Vector3.up;
        Quaternion spawnRotation = _monster.transform.rotation;

        // ObjectPooler에서 SkillData의 이름(태그)으로 스킬 오브젝트를 가져옴
        GameObject skillObj = ObjectPooler.Instance.GetFromPool(skillToUse.skillName, spawnPos, spawnRotation);
        if (skillObj != null)
        {
            // 스폰된 오브젝트에서 'Skill' 컴포넌트를 찾음
            Skill skill = skillObj.GetComponent<Skill>();
            if (skill != null)
            {
                // 'Skill'의 표준 방식인 Activate 함수를 호출
                skill.Activate(_monster.gameObject, skillToUse);
            }
        }
    }

    public override void ExitState() { }
}