using UnityEngine;

public class SpreadFirePattern : IFirePattern
{
    public void Execute(GameObject caster, SkillData data, Vector3 spawnPos, Quaternion baseRotation, Vector3 targetPoint)
    {
        if (data is SpreadShotSkillData spreadData)
        {
            int count = spreadData.numberOfProjectiles;
            float totalAngle = (count - 1) * spreadData.angleBetweenProjectiles;
            float startAngle = -totalAngle / 2f;

            for (int i = 0; i < count; i++)
            {
                float currentAngle = startAngle + i * spreadData.angleBetweenProjectiles;
                // 기본 발사 각도에서 현재 각도만큼 추가로 회전시킴
                Quaternion rotation = baseRotation * Quaternion.Euler(0, currentAngle, 0);

                GameObject skillObj = ObjectPooler.Instance.GetFromPool(spreadData.skillName, spawnPos, rotation);
                skillObj?.GetComponent<Skill>()?.Activate(caster, spreadData);
            }
        }
        else
        {
            // 혹시 잘못된 데이터가 들어오면, 안전하게 단일 발사로 처리
            new SingleFirePattern().Execute(caster, data, spawnPos, baseRotation, targetPoint);
        }
    }
}