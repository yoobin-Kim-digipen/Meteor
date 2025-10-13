using UnityEngine;

public interface IFirePattern
{
    // caster: 스킬을 쏜 주체
    // data: 사용할 스킬의 모든 정보
    // spawnPos: 스킬이 시작될 위치
    // baseRotation: 스킬의 기본 발사 방향
    void Execute(GameObject caster, SkillData data, Vector3 spawnPos, Quaternion baseRotation, Vector3 targetPoint);
}