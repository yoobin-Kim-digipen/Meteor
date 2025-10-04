using UnityEngine;

public class MonsterMeleeAttackState : MonsterBaseState
{
    private float _attackCooldownTimer;
    private MeleeMonsterData _meleeData;

    public MonsterMeleeAttackState(MonsterStateMachine context, MonsterStateFactory factory) : base(context, factory)
    {
        _meleeData = _monster.monsterData as MeleeMonsterData;
    }

    public override void EnterState()
    {
        //Debug.Log("몬스터: 공격 시작!");
        _attackCooldownTimer = 0f; // 쿨타임 초기화
        _monster.agent.isStopped = true;
    }

    public override void UpdateState()
    {
        if (_monster.target == null || _meleeData == null)
        {
            _ctx.SwitchState(_factory.Chase());
            return;
        }

        if (!_monster.IsPlayerInAttackRange())
        {
            _ctx.SwitchState(_factory.Chase());
            return;
        }

        // 쿨타임마다 공격 실행
        _attackCooldownTimer -= Time.deltaTime;
        if (_attackCooldownTimer <= 0f)
        {
            PerformMeleeAttack();
        }
    }

    public override void ExitState()
    {
        _monster.agent.isStopped = false;
    }

    private void PerformMeleeAttack()
    {
        _attackCooldownTimer = _meleeData.attackCooldown;
        // _monster.Animator.SetTrigger("Attack");

        // 공격을 시도할때 플레이어를 바라보게 함
        Vector3 lookDirection = (_monster.target.position - _monster.transform.position);
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
        {
            _monster.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        // --- 전방 부채꼴 범위 체크 ---
        Vector3 directionToPlayer = (_monster.target.position - _monster.transform.position).normalized;
        float angle = Vector3.Angle(_monster.transform.forward, directionToPlayer);

        // [공격 성공!] 플레이어가 정면에 있습니다.
        if (angle <= _meleeData.attackAngle / 2)
        {
            Debug.Log($"<color=orange>{_monster.name}의 근접 공격 성공!</color> 플레이어가 정면에 있습니다. (각도: {angle:F1}도)");
        }
        else
        {
            // [공격 실패!] 플레이어가 정면에 없습니다 (헛스윙).
            Debug.Log($"{_monster.name}의 헛스윙! 플레이어가 공격 범위를 벗어났습니다. (각도: {angle:F1}도)");
        }
    }
}