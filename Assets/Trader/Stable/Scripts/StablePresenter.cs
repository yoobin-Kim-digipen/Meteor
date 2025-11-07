using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class StablePresenter
{
    private readonly IStableView view;

    // --- Model 데이터 캐시 및 현재 상태 ---
    private PlayerWagon currentWagon;
    private Dictionary<ItemData, int> playerInventory;
    private object selectedItem;
    private StableUIMode currentMode = StableUIMode.Upgrading; // UI 시작 시 기본 모드
    private UpgradeFilter currentFilter = UpgradeFilter.All;   // UI 시작 시 기본 필터

    public StablePresenter(IStableView view)
    {
        this.view = view;
        ConnectEvents();
    }

    private void ConnectEvents()
    {
        view.OnModeTabClicked += SetMode;
        view.OnFilterClicked += SetFilter;
        view.OnBlueprintSelected += (blueprint) => HandleItemSelection(blueprint);
        view.OnPartSelected += (part) => HandleItemSelection(part);
        view.OnActionButtonClicked += HandleActionButtonClick;
        view.OnExitClicked += HandleExitRequest;
    }

    public void OnViewEnabled()
    {
        LoadModelData();
        if (currentWagon == null)
        {
            Debug.LogError("[Presenter] 표시할 마차 데이터를 찾을 수 없습니다. StableManager를 확인하세요.");
            // 여기서 더 진행하지 않고 UI에 오류를 표시하거나 UI를 닫을 수 있습니다.
            return;
        }
        // UI를 처음 켤 때는 목록의 첫 번째 항목을 자동으로 선택하도록 합니다.
        RefreshUI(true);
    }

    private void LoadModelData()
    {
        currentWagon = StableManager.Instance.GetCurrentWagon();
        playerInventory = StableManager.Instance.playerInventory;
    }

    private void SetMode(StableUIMode newMode)
    {
        if (currentMode == newMode) return;

        currentMode = newMode;
        selectedItem = null; // 모드가 바뀌면 선택이 초기화됩니다.
        RefreshUI(true); // 새 목록의 첫 항목을 자동으로 선택합니다.
    }

    private void SetFilter(UpgradeFilter newFilter)
    {
        if (currentFilter == newFilter) return;

        currentFilter = newFilter;
        selectedItem = null; // 필터가 바뀌면 선택이 초기화됩니다.
        RefreshUI(true); // 새 목록의 첫 항목을 자동으로 선택합니다.
    }

    private void RefreshUI(bool isAutoSelectFirst = false)
    {
        object itemToReselect = selectedItem; // 액션 후 선택 유지를 위해 이전 선택 항목을 기억

        // 1. 탭, 필터 등 UI 상태 설정
        view.SetActiveMode(currentMode);
        view.SetActiveFilter(currentFilter);

        // 2. 대시보드 및 재화 정보 업데이트 (Presenter는 순수 데이터만 전달)
        view.SetPlayerCurrency(StableManager.Instance.playerGold);
        view.UpdateWagonDashboard(
             currentWagon.baseData.wagonName,
             currentWagon.currentBasicUpgradeLevel,
             currentWagon.baseData.maxBasicUpgradeLevel,
             currentWagon.equippedDestinyParts.Count,
             currentWagon.baseData.destinyUpgradeSlots,
             currentWagon.currentLoad,
             currentWagon.maxLoad
         );
        view.UpdateWagonDisplay(currentWagon.baseData.image);

        // 3. 목록 채우기
        List<object> populatedItems = PopulateList();

        // 4. 상세 정보 패널 및 선택 상태 결정
        if (isAutoSelectFirst) // 모드/필터 변경 시
        {
            if (populatedItems.Any()) HandleItemSelection(populatedItems.First());
            else view.ClearDetailPanel(); // 목록이 비었으면 상세 패널도 비움
        }
        else if (itemToReselect != null && populatedItems.Contains(itemToReselect)) // 액션 성공 후
        {
            // 이전에 선택한 아이템이 여전히 목록에 있다면 다시 선택
            HandleItemSelection(itemToReselect);
        }
        else // 액션 성공 후 이전에 선택한 아이템이 목록에서 사라졌다면
        {
            selectedItem = null;
            view.ClearDetailPanel();
        }
    }

    #region List Population

    private List<object> PopulateList()
    {
        view.ClearList();
        return currentMode == StableUIMode.Crafting ? PopulateBlueprintList() : PopulatePartList();
    }

    private List<object> PopulateBlueprintList()
    {
        var allBlueprints = StableManager.Instance.GetAllBlueprints();
        if (allBlueprints == null) return new List<object>();

        foreach (var blueprint in allBlueprints)
        {
            bool canCraft = StableManager.Instance.CanCraftWagon(blueprint);
            view.AddBlueprintToList(blueprint, canCraft);
        }
        return allBlueprints.Cast<object>().ToList();
    }

    private List<object> PopulatePartList()
    {
        var allParts = StableManager.Instance.allAvailableParts;
        if (allParts == null) return new List<object>();

        var filteredParts = allParts
            .Where(part =>
                   currentFilter == UpgradeFilter.All ||
                   (currentFilter == UpgradeFilter.Basic && part.partType == PartType.Basic) ||
                   (currentFilter == UpgradeFilter.Destiny && part.partType == PartType.Destiny))
            .ToList();

        foreach (var part in filteredParts)
        {
            PartStatus status = StableManager.Instance.GetPartStatusForWagon(currentWagon, part);
            view.AddPartToList(part, status);
        }
        return filteredParts.Cast<object>().ToList();
    }
    #endregion

    #region Item Selection & Actions

    private void DeselectCurrentItem()
    {
        if (selectedItem == null) return;

        if (selectedItem is WagonBlueprintData oldBlueprint) view.SetBlueprintSelection(oldBlueprint, false);
        else if (selectedItem is PartData oldPart) view.SetPartSelection(oldPart, false);
    }

    private void HandleItemSelection(object item)
    {
        if (item == null) return;

        DeselectCurrentItem();
        selectedItem = item;

        if (selectedItem is WagonBlueprintData newBlueprint)
        {
            view.SetBlueprintSelection(newBlueprint, true);
            view.ShowBlueprintDetails(newBlueprint, playerInventory);
            bool canCraft = StableManager.Instance.CanCraftWagon(newBlueprint);
            view.UpdateActionButton(canCraft, "제작하기");
        }
        else if (selectedItem is PartData newPart)
        {
            view.SetPartSelection(newPart, true);
            PartStatus status = StableManager.Instance.GetPartStatusForWagon(currentWagon, newPart);

            if (status == PartStatus.Craftable || status == PartStatus.Locked)
                view.ShowPartDetails(newPart, playerInventory);
            else // Equipped or Unequipped
                view.ShowEquippedPartDetails(newPart);

            UpdateActionButtonState(status, newPart);
        }
    }

    private void HandleActionButtonClick()
    {
        if (selectedItem == null) return;

        bool actionSuccess = false;

        if (selectedItem is WagonBlueprintData blueprint)
        {
            actionSuccess = StableManager.Instance.CraftWagon(blueprint);
        }
        else if (selectedItem is PartData part)
        {
            PartStatus status = StableManager.Instance.GetPartStatusForWagon(currentWagon, part);
            switch (status)
            {
                case PartStatus.Craftable:
                    actionSuccess = StableManager.Instance.CraftPart(currentWagon, part);
                    break;
                case PartStatus.Equipped:
                    actionSuccess = StableManager.Instance.UnequipPart(currentWagon, part);
                    break;
                case PartStatus.Unequipped:
                    actionSuccess = StableManager.Instance.EquipPart(currentWagon, part);
                    break;
            }
        }

        if (actionSuccess)
        {
            LoadModelData(); // Model의 데이터가 변경되었으므로 다시 로드
            RefreshUI(false); // UI 새로고침 (선택 유지 시도)
        }
    }

    private void UpdateActionButtonState(PartStatus status, PartData part)
    {
        switch (status)
        {
            case PartStatus.Equipped:
                view.UpdateActionButton(true, "장착 해제");
                break;
            case PartStatus.Unequipped:
                view.UpdateActionButton(true, "장착하기");
                break;
            case PartStatus.Craftable:
                bool canCraft = StableManager.Instance.CanCraft(part);
                string text = part.partType == PartType.Basic ? "개조하기" : "제작하기";
                view.UpdateActionButton(canCraft, text);
                break;
            case PartStatus.Locked:
                view.UpdateActionButton(false, "조건 미충족");
                break;
        }
    }

    private void HandleExitRequest()
    {
        UIManager.Instance.CloseStablePanel();
    }
    #endregion
}