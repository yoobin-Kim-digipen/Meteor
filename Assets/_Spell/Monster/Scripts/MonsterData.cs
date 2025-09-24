using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "New MonsterData", menuName = "Monsters/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Info")]
    public string monsterName;
    public GameObject monsterPrefab; // 스폰할 몬스터의 프리팹

    [Header("Stats")]
    public float health = 100f;
    public float speed = 3.5f;
    //public float detectionRadius = 10f;
    //public float damage = 10f;

    [Header("Pooling")]
    public int poolSize = 50; // 이 몬스터를 위해 미리 생성해 둘 개수

    [Header("Spawning")]
    [Tooltip("이 몬스터가 스폰되는 주기 (초)")]
    public float spawnInterval = 5f; // 몬스터별 스폰 간격

    [Tooltip("씬에 동시에 존재할 수 있는 이 몬스터의 최대 수")]
    public int maxAliveCount = 10; // 몬스터별 최대 개체 수
}