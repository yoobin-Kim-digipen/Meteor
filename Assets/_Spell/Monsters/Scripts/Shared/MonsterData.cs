using UnityEngine;

public class MonsterData : ScriptableObject
{
    [Header("Info")]
    public string monsterName;
    public GameObject monsterPrefab;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float speed = 3.5f;
    public float attackRange = 15f;

    [Header("Spawning")]
    public int poolSize = 10;
    public float spawnInterval = 5f;
    public int maxAliveCount = 5;
}