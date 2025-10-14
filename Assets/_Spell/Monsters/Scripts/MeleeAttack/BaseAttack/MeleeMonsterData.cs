using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Monster", menuName = "Monsters/Melee Monster Data")]
public class MeleeMonsterData : MonsterData
{
    [Header("Melee Attack Stats")]
    public float attackWindupTime = 0.5f;
    public float attackCooldown = 1.5f;
    public float damage = 15f;
    public float attackAngle = 45f;     // 공격 부채꼴의 총 각도
    public float attackHeight = 2.0f;
}