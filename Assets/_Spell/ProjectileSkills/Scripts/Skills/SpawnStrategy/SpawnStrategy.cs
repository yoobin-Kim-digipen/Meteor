using UnityEngine;

public abstract class SpawnStrategy : ScriptableObject
{
    // caster: 스킬을 쏜 주체(플레이어 또는 몬스터)의 Transform
    // targetPoint: 조준점 (플레이어의 경우) 또는 타겟의 위치 (몬스터의 경우)
    // skillData: 이 계산에 필요한 추가 정보 (예: 높이 오프셋)
    // out spawnPos, out spawnRot: 이 함수가 계산해서 돌려줄 '결과값' (스폰 위치와 회전)
    public abstract void CalculateSpawnTransform(Transform caster, Vector3 targetPoint, SkillData skillData, out Vector3 spawnPos, out Quaternion spawnRot);
}