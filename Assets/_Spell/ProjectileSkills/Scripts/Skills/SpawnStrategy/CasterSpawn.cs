using UnityEngine;

[CreateAssetMenu(fileName = "CasterSpawnStrategy", menuName = "Skills/Spawn Strategies/From Caster")]
public class CasterSpawn : SpawnStrategy
{
    public override void CalculateSpawnTransform(Transform caster, Vector3 targetPoint, SkillData skillData, out Vector3 spawnPos, out Quaternion spawnRot)
    {
        // 시전자의 현재 위치와 회전을 기준으로, SkillData에 정의된 오프셋을 적용
        spawnPos = caster.position + caster.rotation * skillData.spawnOffset;

        // 계산된 스폰 위치에서 목표 지점을 바라보도록 회전값을 계산
        spawnRot = Quaternion.LookRotation((targetPoint - spawnPos).normalized);
    }
}
