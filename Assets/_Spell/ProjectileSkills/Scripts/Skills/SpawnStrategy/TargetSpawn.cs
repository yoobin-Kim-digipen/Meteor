using UnityEngine;

[CreateAssetMenu(fileName = "TargetSpawnStrategy", menuName = "Skills/Spawn Strategies/On Target")]
public class TargetSpawn : SpawnStrategy
{
    public override void CalculateSpawnTransform(Transform caster, Vector3 targetPoint, SkillData skillData, out Vector3 spawnPos, out Quaternion spawnRot)
    {
        // 목표 지점(targetPoint)을 기준으로, SkillData에 정의된 높이 오프셋을 적용
        spawnPos = targetPoint + Vector3.up * skillData.spawnHeightOffset;

        // 메테오처럼 위에서 아래로 떨어지는 스킬을 위해, 아래를 바라보도록 회전값을 고정
        spawnRot = Quaternion.LookRotation(Vector3.down);
    }
}