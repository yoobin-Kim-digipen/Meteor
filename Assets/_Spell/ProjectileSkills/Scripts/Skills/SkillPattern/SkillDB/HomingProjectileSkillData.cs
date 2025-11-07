using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Homing Projectile Skill", menuName = "Skills/Homing Projectile Skill")]
public class HomingProjectileSkillData : SkillData // 기존 SkillData 상속
{
    [Header("Projectile Stats")]
    public float speed;         // 이동 속도
    public float lifetime;      // 생존 시간
    public float damage;        // 피해량

    [Header("Homing Settings")]
    [Tooltip("유도탄이 처음 대상을 찾을 반경 (이후에는 추적)")]
    public float targetSearchRadius = 15f;
    [Tooltip("대상을 향해 회전하는 속도")]
    public float rotationSpeed = 5f;
    [Tooltip("한 번에 찾을 수 있는 최대 타겟 수")]
    public int maxTargetsToFind = 1;

    [Header("On-Hit Effects")]
    [Tooltip("이 발사체에 맞았을 때 적용될 모든 상태 이상 효과 목록")]
    public List<StatusEffect> onHitEffects = new List<StatusEffect>();

    public override IFirePattern GetFirePattern()
    {
        return new SingleFirePattern(); // 유도탄도 기본적으로 단일 발사
    }
}