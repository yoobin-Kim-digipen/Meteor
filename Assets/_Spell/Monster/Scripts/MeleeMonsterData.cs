using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Monster", menuName = "Monsters/Melee Monster Data")]
public class MeleeMonsterData : MonsterData
{
    [Header("Melee Attack Stats")]
    public float attackCooldown = 1.5f;
    public float damage = 15f;

    // 나중에 근접 공격의 범위를 정하기 위해 attackRange를 사용
    // MonsterData에 이미 attackRange가 있으므로 여기에는 추가할 필요가 없음
}