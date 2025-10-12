using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Projectile Skill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkillData : SkillData
{
    [Header("Projectile Stats")]
    public float speed;
    public float lifetime;
    public float damage;

    [Header("On-Hit Effects")]
    [Tooltip("이 발사체에 맞았을 때 적용될 모든 상태 이상 효과 목록")]
    public List<StatusEffect> onHitEffects = new List<StatusEffect>();

    public override IFirePattern GetFirePattern()
    {
        return new SingleFirePattern();
    }
}