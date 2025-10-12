using UnityEngine;

[CreateAssetMenu(fileName = "New Burn Effect", menuName = "Status Effects/Burn Effect")]
public class BurnEffect : StatusEffect // StatusEffect 설계도를 상속
{
    [Header("Burn Specifics")]
    [Tooltip("틱 당 입힐 데미지")]
    public float damagePerTick = 5f;

    [Tooltip("데미지가 들어오는 시간 간격 (초)")]
    public float tickInterval = 1f;

    // 'StatusEffect'의 규칙(ApplyEffect 함수)을 실제로 구현
    public override void ApplyEffect(CharacterStatus targetStatus)
    {
        if (targetStatus != null)
        {
            // CharacterStatus에게 "이 값들로 화상 효과를 적용해줘" 라고 명령
            targetStatus.ApplyBurn(damagePerTick, tickInterval, duration);
        }
    }
}