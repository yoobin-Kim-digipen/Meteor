using UnityEngine;

[CreateAssetMenu(fileName = "New Rockfall Skill", menuName = "Skills/Rockfall Skill")]
public class RockfallSkillData : SkillData
{
    [Header("Rockfall Stats")]
    public float damage = 100f; // 단일 대상에게 줄 데미지

    // 이 스킬은 '단일 오브젝트 생성'이므로, 가장 기본적인 SingleFirePattern을 사용.
    public override IFirePattern GetFirePattern()
    {
        return new SingleFirePattern();
    }
}