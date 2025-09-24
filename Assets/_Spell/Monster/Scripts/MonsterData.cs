using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "New MonsterData", menuName = "Monsters/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Info")]
    public string monsterName;
    public GameObject monsterPrefab; // ������ ������ ������

    [Header("Stats")]
    public float health = 100f;
    public float speed = 3.5f;
    //public float detectionRadius = 10f;
    //public float damage = 10f;

    [Header("Pooling")]
    public int poolSize = 50; // �� ���͸� ���� �̸� ������ �� ����

    [Header("Spawning")]
    [Tooltip("�� ���Ͱ� �����Ǵ� �ֱ� (��)")]
    public float spawnInterval = 5f; // ���ͺ� ���� ����

    [Tooltip("���� ���ÿ� ������ �� �ִ� �� ������ �ִ� ��")]
    public int maxAliveCount = 10; // ���ͺ� �ִ� ��ü ��
}