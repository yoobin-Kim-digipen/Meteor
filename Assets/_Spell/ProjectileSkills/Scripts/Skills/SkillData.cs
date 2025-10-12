using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    [Header("Common Info")]
    public string skillName;
    public GameObject skillPrefab;
    public int poolSize = 20;

    [Header("Spawning Behavior")]
    public SpawnStrategy spawnStrategy;
    public Vector3 spawnOffset = Vector3.up;
    public float spawnHeightOffset = 20f;

    public abstract IFirePattern GetFirePattern();
}