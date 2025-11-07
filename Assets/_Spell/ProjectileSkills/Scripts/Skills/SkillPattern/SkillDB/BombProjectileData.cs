using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New BombProjectile Skill", menuName = "Skills/BombProjectile Skill")]
public class BombProjectileSkillData : SkillData
{
    [Header("Projectile Stats")]
    public float speed;
    public float lifetime;
    public float damage;

    //[Header("On-Hit Effects")]
    //[Tooltip("이 발사체에 맞았을 때 적용될 모든 상태 이상 효과 목록")]
    //public List<StatusEffect> onHitEffects = new List<StatusEffect>();

    [Header("Explosion Settings")]
    [Tooltip("폭발 시 생성될 파티클 효과 프리팹")]
    public GameObject explosionPrefab;
    [Tooltip("광역 데미지가 적용될 반경. 0 이하면 광역 데미지가 없습니다.")]
    public float explosionRadius = 0f;

    public override IFirePattern GetFirePattern()
    {
        return new SingleFirePattern();
    }
}