using UnityEngine;
public class ActionPhase : IAttackPhase
{
    private MonsterMeleeAttackState _superState;
    private bool _hasDealtDamage; // 데미지를 한 번만 주도록 체크하는 플래그

    public ActionPhase(MonsterMeleeAttackState superState)
    {
        _superState = superState;
    }

    public void EnterPhase()
    {
        _hasDealtDamage = false; // 단계 진입 시 플래그 초기화

        // (애니메이션) 실제 공격 애니메이션 재생
        // _superState.Monster.Animator.SetTrigger("AttackAction");
    }

    public void UpdatePhase()
    {
        // 이 단계에서는 진입 후 첫 프레임에 딱 한 번만 데미지 판정을 수행
        if (!_hasDealtDamage)
        {
            PerformMeleeAttack();
            _hasDealtDamage = true;
        }

        // 애니메이션이 없는 현재는, 판정 후 즉시 회복 단계로 전환.
        // (나중에 애니메이션 길이에 맞춰 전환 타이밍을 조절하면 더 자연스러워짐)
        _superState.SwitchPhase(typeof(RecoveryPhase));
    }

    public void ExitPhase() { }

    private void PerformMeleeAttack()
    {
        if (_superState.Monster.target == null) return;

        // 조건 1: 거리 (3D 거리)
        float distance = Vector3.Distance(_superState.Monster.transform.position, _superState.Monster.target.position);
        bool isDistanceOK = distance <= _superState.MeleeData.attackRange;

        // 조건 2: 각도 (수평 각도) - attackAngle이 360 이상이면 이 조건은 항상 통과됨
        Vector3 directionToPlayer3D = _superState.Monster.target.position - _superState.Monster.transform.position;
        Vector3 directionToPlayer2D = new Vector3(directionToPlayer3D.x, 0, directionToPlayer3D.z);
        float angle = (directionToPlayer2D.sqrMagnitude > 0.001f) ? Vector3.Angle(_superState.Monster.transform.forward, directionToPlayer2D.normalized) : 0f;
        bool isAngleOK = angle <= _superState.MeleeData.attackAngle / 2;

        // 조건 3: 높이
        float heightDifference = Mathf.Abs(directionToPlayer3D.y);
        bool isHeightOK = heightDifference <= _superState.MeleeData.attackHeight;

        if (isDistanceOK && isAngleOK && isHeightOK)
        {
            Debug.Log($"<color=orange>{_superState.Monster.name}의 근접 공격 성공!</color>");
            StatManager.Instance.playerHealth.TakeDamage(_superState.MeleeData.damage);
            bool isParried = false; // 임시
            if (!isParried)
            {
                ApplySpecialEffect();
            }
        }
        else
        {
            Debug.Log($"{_superState.Monster.name}의 헛스윙! (거리OK: {isDistanceOK}, 각도OK: {isAngleOK}, 높이OK: {isHeightOK})");
        }
    }

    private void ApplySpecialEffect()
    {
        // 약탈 망령
        if (_superState.MeleeData is LootingMonsterData lootingData)
        {
            // "마력 결정을 1개 잃었습니다!" 로그 출력
            Debug.Log($"<color=magenta>특수 효과 발동!</color> 마력 결정을 {lootingData.manaCrystalDrain}개 잃었습니다!");

        }
        // 냉기 망령
        else if (_superState.MeleeData is FrostMonsterData frostData)
        {
            // "이동속도가 3초간 30% 감소합니다!" 로그 출력
            Debug.Log($"<color=cyan>특수 효과 발동!</color> 이동속도가 {frostData.slowDuration}초간 {frostData.slowAmount * 100}% 감소합니다!");

        }
    }
}
