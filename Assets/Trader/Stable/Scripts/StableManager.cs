using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public struct TestInventoryItem
{
    public StableItemData itemData;
    public int amount;
}

public enum PartStatus { Equipped, Unequipped, Craftable, Locked }

public class StableManager : MonoBehaviour
{
    public static StableManager Instance;

    [Header("게임 전체 데이터")]
    public List<PartData> allAvailableParts;
    public List<WagonBlueprintData> allBlueprints;

    [Header("플레이어 데이터 (임시)")]
    public PlayerWagon currentPlayerWagon;
    public List<PartData> ownedDestinyParts = new List<PartData>();
    public Dictionary<StableItemData, int> playerInventory = new Dictionary<StableItemData, int>();
    public int playerGold = 9999;

    [Header("테스트용 시작 아이템")]
    public List<TestInventoryItem> startingInventory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        playerInventory.Clear();
        foreach (var testItem in startingInventory)
        {
            if (testItem.itemData != null && !playerInventory.ContainsKey(testItem.itemData))
            {
                playerInventory.Add(testItem.itemData, testItem.amount);
            }
        }
    }

    public List<WagonBlueprintData> GetAllBlueprints()
    {
        var ownedWagonBases = new List<WagonBaseData> { currentPlayerWagon.baseData };
        return allBlueprints.Where(blueprint => !ownedWagonBases.Contains(blueprint.resultWagonBaseData)).ToList();
    }

    public PlayerWagon GetCurrentWagon()
    {
        if (currentPlayerWagon == null) return null;
        currentPlayerWagon.Initialize();
        return currentPlayerWagon;
    }

    #region Status Check Logic (수정된 부분)

    // 특정 부품의 현재 상태를 판단하는 함수 (확인 순서 변경)
    public PartStatus GetPartStatusForWagon(PlayerWagon wagon, PartData part)
    {
        if (wagon == null || part == null) return PartStatus.Locked;

        // 1. 운명 부품이라면, 보유/장착 상태를 먼저 확인한다.
        if (part.partType == PartType.Destiny)
        {
            if (wagon.equippedDestinyParts.Contains(part)) return PartStatus.Equipped;
            if (ownedDestinyParts.Contains(part)) return PartStatus.Unequipped;
        }

        // 2. 위에서 걸러지지 않았다면, 이제 제작이 가능한지 확인한다.
        if (CanCraft(part)) return PartStatus.Craftable;

        // 3. 모든 조건에 해당하지 않으면 잠금 상태이다.
        return PartStatus.Locked;
    }

    // 부품 제작 가능 여부 확인 함수 (중복 제작 방지 로직 추가)
    public bool CanCraft(PartData part)
    {
        // 운명 부품 중복 제작 방지
        if (part.partType == PartType.Destiny)
        {
            // 이미 장착했거나, 제작해서 보유 중인 부품은 더 이상 제작할 수 없다.
            if (currentPlayerWagon.equippedDestinyParts.Contains(part) || ownedDestinyParts.Contains(part))
            {
                return false;
            }
        }

        // 기존 재료/골드 확인 로직
        if (playerGold < part.requiredGold) return false;
        foreach (var required in part.requiredMaterials)
        {
            if (!playerInventory.ContainsKey(required.item) || playerInventory[required.item] < required.amount) return false;
        }
        return true;
    }

    public bool CanCraftWagon(WagonBlueprintData blueprint)
    {
        if (playerGold < blueprint.requiredGold) return false;
        foreach (var required in blueprint.requiredMaterials)
        {
            if (!playerInventory.ContainsKey(required.item) || playerInventory[required.item] < required.amount) return false;
        }
        return true;
    }
    #endregion

    #region Action Logic (변경 없음)
    public bool CraftWagon(WagonBlueprintData blueprint)
    {
        if (blueprint == null || !CanCraftWagon(blueprint)) return false;
        foreach (var required in blueprint.requiredMaterials) playerInventory[required.item] -= required.amount;
        playerGold -= blueprint.requiredGold;
        PlayerWagon newWagon = new PlayerWagon { baseData = blueprint.resultWagonBaseData, currentBasicUpgradeLevel = 0 };
        newWagon.Initialize();
        currentPlayerWagon = newWagon;
        Debug.Log($"<color=cyan>마차 업그레이드 성공!</color> '{newWagon.baseData.wagonName}'(으)로 교체되었습니다.");
        return true;
    }

    public bool CraftPart(PlayerWagon wagon, PartData part)
    {
        if (wagon == null || !CanCraft(part)) return false;
        foreach (var required in part.requiredMaterials) playerInventory[required.item] -= required.amount;
        playerGold -= part.requiredGold;
        if (part.partType == PartType.Basic)
        {
            wagon.currentBasicUpgradeLevel++;
            Debug.Log($"<color=cyan>기본 개조 성공!</color> '{part.partName}' 레벨이 올랐습니다. (현재: {wagon.currentBasicUpgradeLevel})");
        }
        else if (part.partType == PartType.Destiny)
        {
            ownedDestinyParts.Add(part);
            Debug.Log($"<color=cyan>운명 부품 제작 성공!</color> '{part.partName}'을 획득했습니다.");
        }
        return true;
    }

    public bool EquipPart(PlayerWagon wagon, PartData part)
    {
        if (wagon == null || part.partType != PartType.Destiny || !ownedDestinyParts.Contains(part)) return false;
        if (wagon.equippedDestinyParts.Count >= wagon.baseData.destinyUpgradeSlots) return false;
        ownedDestinyParts.Remove(part);
        wagon.equippedDestinyParts.Add(part);
        Debug.Log($"<color=yellow>장착 완료!</color> '{part.partName}'을(를) '{wagon.baseData.wagonName}'에 장착했습니다.");
        return true;
    }

    public bool UnequipPart(PlayerWagon wagon, PartData part)
    {
        if (wagon == null || !wagon.equippedDestinyParts.Contains(part)) return false;
        wagon.equippedDestinyParts.Remove(part);
        ownedDestinyParts.Add(part);
        Debug.Log($"<color=gray>장착 해제.</color> '{part.partName}'을(를) '{wagon.baseData.wagonName}'에서 제거했습니다.");
        return true;
    }
    #endregion
}