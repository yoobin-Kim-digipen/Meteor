using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Skill", menuName = "Skills/Projectile Skill")]
public class ProjectileSkillData : SkillData
{
    [Header("Projectile Stats")]
    public float speed;
    public float lifetime;
    public float damage;
}