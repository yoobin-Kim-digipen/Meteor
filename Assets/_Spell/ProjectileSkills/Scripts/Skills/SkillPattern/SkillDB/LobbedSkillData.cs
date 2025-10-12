using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Lobbed Skill", menuName = "Skills/Lobbed Skill")]
public class LobbedSkillData : SkillData
{
    [Header("Lobbed Projectile Stats")]
    public float launchAngle = 45f; // 발사 각도
    public float damage = 50f;
    public float explosionRadius = 4f;
    public float knockbackForce = 10f;

    [Header("On-Hit Effects")]
    public List<StatusEffect> onHitEffects = new List<StatusEffect>();
    public override IFirePattern GetFirePattern()
    {
        return new LobbedFirePattern();
    }
}