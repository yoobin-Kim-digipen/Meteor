// Scripts/Managers/StableManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq 사용을 위해 추가

[System.Serializable]
public struct TestInventoryItem
{
    public ItemData itemData; // 재료의 Scriptable Object 에셋
    public int amount;      // 재료의 수량
}

public class StableManager : MonoBehaviour
{
    public static StableManager Instance;

    [Header("게임 전체 데이터 (에디터에서 연결)")]
    public List<PartData> allAvailableParts; // 2단계에서 만든 모든 Part 에셋들

    [Header("플레이어 데이터 (임시)")]
    public List<PlayerWagon> playerWagons = new List<PlayerWagon>();
    public Dictionary<ItemData, int> playerInventory = new Dictionary<ItemData, int>();
    public int playerGold = 9999; // 테스트용 골드

    public List<TestInventoryItem> startingInventory;

    private int currentWagonIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // --- 테스트용 데이터 초기화 ---
        playerInventory = new Dictionary<ItemData, int>(); // 딕셔너리 초기화
        foreach (var testItem in startingInventory)
        {
            // 인스펙터에서 연결한 ItemData가 null이 아닌지 확인
            if (testItem.itemData != null)
            {
                playerInventory.Add(testItem.itemData, testItem.amount);
            }
        }
    }

    // 현재 마구간에서 보고 있는 마차를 반환
    public PlayerWagon GetCurrentViewingWagon()
    {
        if (playerWagons.Count == 0) return null; // 마차가 없으면 null
        return playerWagons[currentWagonIndex];
    }

    // 특정 부품의 현재 상태를 판단해서 알려주는 함수
    public PartStatus GetPartStatusForWagon(PlayerWagon wagon, PartData part)
    {
        if (wagon == null || part == null) return PartStatus.Locked;

        // 1. 운명 부품이고, 이미 장착했는가?
        if (part.partType == PartType.Destiny && wagon.equippedDestinyParts.Contains(part))
        {
            return PartStatus.Equipped;
        }

        // 2. 재료와 돈이 충분해서 제작 가능한가?
        if (CanCraft(part))
        {
            return PartStatus.Craftable;
        }

        // 3. 둘 다 아니면 그냥 제작 불가능 상태
        return PartStatus.Locked;
    }

    // 제작 가능 여부 확인
    public bool CanCraft(PartData part)
    {
        if (playerGold < part.requiredGold) return false;

        foreach (var required in part.requiredMaterials)
        {
            if (!playerInventory.ContainsKey(required.item) || playerInventory[required.item] < required.amount)
            {
                return false;
            }
        }
        return true;
    }

    // TODO: 실제 제작/장착 로직 구현
}

// 부품 상태를 나타내는 enum
public enum PartStatus { Equipped, Craftable, Locked }