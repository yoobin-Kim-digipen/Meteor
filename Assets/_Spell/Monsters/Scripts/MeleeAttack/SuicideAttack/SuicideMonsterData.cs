using UnityEngine;

[CreateAssetMenu(fileName = "New Suicide Monster", menuName = "Monsters/Suicide Monster Data")]
public class SuicideMonsterData : MonsterData // 일반 MonsterData를 상속
{
    [Header("Suicide Attack Stats")]
    public float damage = 70f;              // 자폭 피해량
    public float explosionRadius = 3.0f;    // 자폭 반경
    public float explosionHeight = 3.0f;    // 자폭 높이 (돔형 판정을 위해)
    public float chargeTime = 0.8f;         // 공격 준비 시간
}