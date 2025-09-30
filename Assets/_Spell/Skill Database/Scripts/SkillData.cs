using UnityEngine;

public enum SkillSpawnType { FromCaster, OnTarget }

public abstract class SkillData : ScriptableObject
{
    [Header("Common Info")]
    public string skillName;
    public GameObject skillPrefab;
    public int poolSize = 20;

    [Header("Spawning Behavior")]
    public SkillSpawnType spawnType = SkillSpawnType.FromCaster;
    public Vector3 spawnOffset = Vector3.up;
    public float spawnHeightOffset = 20f;
}