using UnityEngine;

public class SingleFirePattern : IFirePattern
{
    public void Execute(GameObject caster, SkillData data, Vector3 spawnPos, Quaternion baseRotation, Vector3 targetPoint)
    {
        GameObject skillObj = ObjectPooler.Instance.GetFromPool(data.skillName, spawnPos, baseRotation);
        skillObj?.GetComponent<Skill>()?.Activate(caster, data);
    }
}