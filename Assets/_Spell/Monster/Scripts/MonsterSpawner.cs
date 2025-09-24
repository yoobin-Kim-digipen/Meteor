using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public Transform playerTransform;
    public List<MonsterData> spawnableMonsters; // �� �����ʿ��� ������ ���� ����
    public float maxSpawnRadius = 750f; // �ִ� �ݰ� 
    public float minSpawnRadius = 300f;  // �ּ� �ݰ�
    public int maxMonsters = 30; // ���� ������ �ִ� ���� ��

    private Dictionary<string, List<GameObject>> activeMonsters = new Dictionary<string, List<GameObject>>();
    private int totalMonsterCount = 0;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned in the MonsterSpawner inspector!");
            // �����ʰ� �۵����� �ʵ��� ���⼭ ����
            this.enabled = false;
            return;
        }

        // ���� ������ ��� ���� ������ ���� �ʱ�ȭ �� ���� ���� �ڷ�ƾ ����
        foreach (var monsterData in spawnableMonsters)
        {
            // ��ųʸ� �ʱ�ȭ
            activeMonsters[monsterData.monsterName] = new List<GameObject>();
            // ���� �����ͺ��� ���� �ڷ�ƾ ����
            StartCoroutine(SpawnMonsterCoroutine(monsterData));
        }
    }

    IEnumerator SpawnMonsterCoroutine(MonsterData monsterData)
    {
        while (true)
        {
            // 1. ���� ���͸� ����Ʈ���� �����Ͽ� ���� ���� ���� ��Ȯ�ϰ� ����
            activeMonsters[monsterData.monsterName].RemoveAll(m => !m.activeInHierarchy);
            totalMonsterCount = 0;
            foreach (var list in activeMonsters.Values)
            {
                totalMonsterCount += list.Count;
            }

            // ���� A: �� ���� Ÿ���� ���� Ÿ�Ժ� �ִ�ġ���� ������?
            // ���� B: ��ü ���� ���� ��ü �ִ�ġ���� ������?
            bool canSpawnByType = activeMonsters[monsterData.monsterName].Count < monsterData.maxAliveCount;
            bool canSpawnByTotal = totalMonsterCount < maxMonsters;

            // 2. �ִ� ���� ������ ���� ���� ����
            if (canSpawnByType && canSpawnByTotal)
            {
                SpawnMonster(monsterData);
            }

            // 3. ������ �ð���ŭ ��ٸ�
            yield return new WaitForSeconds(monsterData.spawnInterval);
        }
    }

    void SpawnMonster(MonsterData monsterToSpawn)
    {
        // min ~ max ������ ������ �Ÿ��� ����
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector2 randomCircle = Random.insideUnitCircle.normalized * randomDistance;
        Vector3 randomPosition = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPosition, out hit, maxSpawnRadius, NavMesh.AllAreas))
        {
            Vector3 spawnPosition = hit.position;

            // ���� ���� ��ġ�� ���� �ּ� �Ÿ����� �ָ� �ִ��� Ȯ��
            if (Vector3.Distance(playerTransform.position, spawnPosition) < minSpawnRadius)
            {
                // �� ���� ���� �߻����� �ʰ�����, ������ ���� ������ġ
                return;
            }
            GameObject monster = ObjectPooler.Instance.GetFromPool(monsterToSpawn.monsterName, spawnPosition, Quaternion.identity);

            if (monster != null)
            {
                monster.GetComponent<MonsterFSM>().target = playerTransform;
                activeMonsters[monsterToSpawn.monsterName].Add(monster);
            }
        }
        else
        {
            Debug.LogWarning($"���� ��ġ({randomPosition}) �ֺ��� ��ȿ�� NavMesh�� ã�� ���߽��ϴ�.");
        }
    }
}