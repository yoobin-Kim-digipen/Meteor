using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Count() 같은 Linq 기능을 사용하기 위해 추가

public class LairMonsterSlot
{
    public MonsterData monsterData;   // 어떤 종류의 몬스터가 살아야 하는가
    public Vector3 initialPosition;   // 이 몬스터의 원래 스폰 위치 ('집 주소')
    public MonsterFSM currentMonster; // 현재 이 자리에 할당된 실제 몬스터 FSM
}

public class MonsterLair : MonoBehaviour
{
    [Header("Lair Settings")]
    [Tooltip("이 소굴에서 스폰할 몬스터의 '종류'와 '수'")]
    public List<MonsterData> monsterTypesToSpawn;

    [Tooltip("몬스터들이 활동할 반경 (Sphere Collider와 일치시키는 것이 좋음)")]
    public float activityRadius = 20f;

    [Tooltip("클리어 시 사라질 보상 상자")]
    public GameObject treasureChest;

    [Tooltip("클리어 시 획득할 마력 결정 수")]
    public int manaCrystalReward = 5;

    // --- 내부 관리 변수 ---
    private List<LairMonsterSlot> _monsterSlots = new List<LairMonsterSlot>();
    private bool _isPlayerInLair = false;
    private bool _isCleared = false;
    private Transform _playerTransform;
    private bool _isInitialized = false;

    public bool IsPlayerInLair => _isPlayerInLair;

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (_playerTransform == null)
        {
            Debug.LogError("씬에 'Player' 태그를 가진 오브젝트가 없습니다!", this);
            enabled = false;
            return;
        }

        // 1. 몬스터가 스폰될 '자리'를 먼저 설계
        CreateMonsterSlots();
        // 2. 설계된 자리에 실제 몬스터를 처음으로 스폰
        RespawnMissingMonsters();

        _isInitialized = true;
    }

    void Update()
    {
        if (_isCleared || !_isInitialized) return;

        // 살아있는 몬스터 수를 확인 (Linq 사용)
        int aliveCount = _monsterSlots.Count(slot => slot.currentMonster != null && slot.currentMonster.gameObject.activeInHierarchy);

        // 플레이어가 영역 안에 있고, 살아있는 몬스터가 0마리일 때만 클리어 처리
        if (_isPlayerInLair && aliveCount == 0)
        {
            ClearLair();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInLair = true;
            Debug.Log("플레이어가 소굴에 진입했습니다! 전투 시작!");
            AlertAllMonsters(_playerTransform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInLair = false;
            Debug.Log("플레이어가 소굴을 벗어났습니다! 소굴을 초기화합니다.");
            ResetLair();
        }
    }

    // --- 몬스터 관리 핵심 함수 ---

    // 몬스터가 스폰될 '자리' 목록을 미리 생성하는 함수
    private void CreateMonsterSlots()
    {
        _monsterSlots.Clear();
        foreach (var data in monsterTypesToSpawn)
        {
            if (data == null) continue;

            Vector2 randomCircle = Random.insideUnitCircle * activityRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            _monsterSlots.Add(new LairMonsterSlot
            {
                monsterData = data,
                initialPosition = spawnPosition,
                currentMonster = null // 처음에는 몬스터가 없는 빈 자리
            });
        }
    }

    // 비어있는 모든 자리에 몬스터를 스폰하는 함수
    private void RespawnMissingMonsters()
    {
        foreach (var slot in _monsterSlots)
        {
            // 이 자리에 몬스터가 없거나, 있더라도 비활성화(죽은) 상태라면 새로 스폰
            if (slot.currentMonster == null || !slot.currentMonster.gameObject.activeInHierarchy)
            {
                GameObject monsterObj = ObjectPooler.Instance.GetFromPool(slot.monsterData.monsterName, slot.initialPosition, Quaternion.identity);
                if (monsterObj != null)
                {
                    MonsterFSM fsm = monsterObj.GetComponent<MonsterFSM>();
                    if (fsm != null)
                    {
                        fsm.Initialize(slot.monsterData, this, slot.initialPosition);
                        slot.currentMonster = fsm; // 새로 스폰된 몬스터를 이 자리에 배정
                    }
                }
            }
        }
        Debug.Log("소굴의 몬스터 배치가 완료/갱신되었습니다.");
    }

    // 소굴을 초기화하는 함수 (플레이어가 영역을 벗어났을 때)
    private void ResetLair()
    {
        // 1. 모든 몬스터에게 타겟을 해제하라고 명령
        AlertAllMonsters(null);

        // 2. 살아있는 몬스터는 원래 자리로 복귀시키고, 죽은 몬스터는 리스폰
        foreach (var slot in _monsterSlots)
        {
            if (slot.currentMonster != null && slot.currentMonster.gameObject.activeInHierarchy)
            {
                // [살아있는 몬스터] -> 원래 자리로 복귀 명령
                slot.currentMonster.ReturnToLairPosition();
            }
            else
            {
                // [죽은 몬스터] -> 그 자리에서 새로 스폰
                GameObject monsterObj = ObjectPooler.Instance.GetFromPool(slot.monsterData.monsterName, slot.initialPosition, Quaternion.identity);
                if (monsterObj != null)
                {
                    MonsterFSM fsm = monsterObj.GetComponent<MonsterFSM>();
                    if (fsm != null)
                    {
                        fsm.Initialize(slot.monsterData, this, slot.initialPosition);
                        slot.currentMonster = fsm;
                    }
                }
            }
        }
    }

    // 소굴의 모든 몬스터에게 타겟을 설정/해제
    private void AlertAllMonsters(Transform target)
    {
        foreach (var slot in _monsterSlots)
        {
            if (slot.currentMonster != null)
            {
                slot.currentMonster.SetTarget(target);
            }
        }
    }

    // 소굴이 클리어되었을 때 호출되는 함수
    private void ClearLair()
    {
        _isCleared = true;
        Debug.Log($"<color=green>소굴 클리어!</color> 마력 결정 {manaCrystalReward}개를 획득했습니다.");

        if (treasureChest != null) treasureChest.SetActive(false);
        gameObject.SetActive(false);
    }

    // 기즈모로 활동 범위 표시
    void OnDrawGizmos()
    {
        Gizmos.color = _isPlayerInLair ? Color.yellow : Color.gray;
        Gizmos.DrawWireSphere(transform.position, activityRadius);
    }
}