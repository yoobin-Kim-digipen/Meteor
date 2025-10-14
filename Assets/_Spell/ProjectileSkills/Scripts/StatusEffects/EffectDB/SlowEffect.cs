using UnityEngine;

[CreateAssetMenu(fileName = "New Slow Effect", menuName = "Status Effects/Slow Effect")]
public class SlowEffect : StatusEffect
{
    [Header("Slow Specifics")]
    [Tooltip("이동속도 감소량 (0.3 = 30%)")]

    [Range(0f, 1f)]
    public float slowAmount;

    public override void ApplyEffect(CharacterStatus targetStatus)
    {
        if (targetStatus != null)
        {
            // slowAmount: 이 파일(SlowEffect.asset)에 설정된 둔화량
            // duration: 부모 클래스 StatusEffect에 있는 지속 시간 값
            targetStatus.ApplySlow(slowAmount, duration);
        }
    }
}