using UnityEngine;

[CreateAssetMenu(fileName = "New Frost Monster", menuName = "Monsters/Frost Monster Data")]
public class FrostMonsterData : MeleeMonsterData // 기본 근접 몬스터 데이터를 상속
{
    [Header("Frost Special Ability")]
    [Tooltip("공격 적중 시 적용할 이동속도 감소량 (예: 0.3 = 30%)")]
    public float slowAmount = 0.3f;

    [Tooltip("이동속도 감소 효과의 지속 시간 (초)")]
    public float slowDuration = 3.0f;
}
