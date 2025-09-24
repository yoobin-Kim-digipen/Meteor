using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public Transform playerTransform;
    public List<MonsterData> spawnableMonsters; // 이 스포너에서 스폰할 몬스터 종류
    public float maxSpawnRadius = 750f; // 최대 반경 
    public float minSpawnRadius = 300f;  // 최소 반경
    public int maxMonsters = 30; // 씬에 존재할 최대 몬스터 수

    private Dictionary<string, List<GameObject>> activeMonsters = new Dictionary<string, List<GameObject>>();
    private int totalMonsterCount = 0;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned in the MonsterSpawner inspector!");
            // 스포너가 작동하지 않도록 여기서 멈춤
            this.enabled = false;
            return;
        }

        // 스폰 가능한 모든 몬스터 종류에 대해 초기화 및 개별 스폰 코루틴 시작
        foreach (var monsterData in spawnableMonsters)
        {
            // 딕셔너리 초기화
            activeMonsters[monsterData.monsterName] = new List<GameObject>();
            // 몬스터 데이터별로 개별 코루틴 시작
            StartCoroutine(SpawnMonsterCoroutine(monsterData));
        }
    }

    IEnumerator SpawnMonsterCoroutine(MonsterData monsterData)
    {
        while (true)
        {
            // 1. 죽은 몬스터를 리스트에서 제거하여 현재 몬스터 수를 정확하게 유지
            activeMonsters[monsterData.monsterName].RemoveAll(m => !m.activeInHierarchy);
            totalMonsterCount = 0;
            foreach (var list in activeMonsters.Values)
            {
                totalMonsterCount += list.Count;
            }

            // 조건 A: 이 몬스터 타입의 수가 타입별 최대치보다 적은가?
            // 조건 B: 전체 몬스터 수가 전체 최대치보다 적은가?
            bool canSpawnByType = activeMonsters[monsterData.monsterName].Count < monsterData.maxAliveCount;
            bool canSpawnByTotal = totalMonsterCount < maxMonsters;

            // 2. 최대 몬스터 수보다 적을 때만 스폰
            if (canSpawnByType && canSpawnByTotal)
            {
                SpawnMonster(monsterData);
            }

            // 3. 정해진 시간만큼 기다림
            yield return new WaitForSeconds(monsterData.spawnInterval);
        }
    }

    void SpawnMonster(MonsterData monsterToSpawn)
    {
        // min ~ max 사이의 무작위 거리를 정함
        float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);
        Vector2 randomCircle = Random.insideUnitCircle.normalized * randomDistance;
        Vector3 randomPosition = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomPosition, out hit, maxSpawnRadius, NavMesh.AllAreas))
        {
            Vector3 spawnPosition = hit.position;

            // 최종 스폰 위치가 정말 최소 거리보다 멀리 있는지 확인
            if (Vector3.Distance(playerTransform.position, spawnPosition) < minSpawnRadius)
            {
                // 이 경우는 거의 발생하지 않겠지만, 만약을 위한 안전장치
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
            Debug.LogWarning($"스폰 위치({randomPosition}) 주변에 유효한 NavMesh를 찾지 못했습니다.");
        }
    }
}