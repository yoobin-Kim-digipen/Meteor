using UnityEngine;

[CreateAssetMenu(fileName = "New Spread Shot Skill", menuName = "Skills/Spread Shot Skill")]
public class SpreadShotSkillData : ProjectileSkillData // 기본 투사체 데이터를 상속
{
    [Header("Spread Shot Stats")]
    [Tooltip("총 발사할 투사체의 개수 (홀수를 추천)")]
    public int numberOfProjectiles = 3; // 좌/중/우 3발

    [Tooltip("각 투사체 사이의 각도 (총 부채꼴 각도의 절반이 아님)")]
    public float angleBetweenProjectiles = 15f; // 기획서의 15°

    // SpreadShotSkillData는 '확산탄 발사' 방식을 사용한다고 선언
    public override IFirePattern GetFirePattern()
    {
        return new SpreadFirePattern();
    }
}