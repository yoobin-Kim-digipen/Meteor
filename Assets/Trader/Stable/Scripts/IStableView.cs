// --- 파일명: IStableView.cs ---

using UnityEngine;
using System.Collections.Generic;

// (Delegate, Enum 정의는 변경 없음)
public delegate void BlueprintSelectionHandler(WagonBlueprintData blueprint);
public delegate void PartSelectionHandler(PartData part);
public enum StableUIMode { Crafting, Upgrading }
public enum UpgradeFilter { All, Basic, Destiny }

public interface IStableView
{
    // --- View -> Presenter 이벤트 ---
    event System.Action<StableUIMode> OnModeTabClicked;
    event System.Action<UpgradeFilter> OnFilterClicked;
    event BlueprintSelectionHandler OnBlueprintSelected;
    event PartSelectionHandler OnPartSelected;
    event System.Action OnActionButtonClicked;
    event System.Action OnExitClicked;

    // --- Presenter -> View 명령 (데이터 타입을 원본으로 변경) ---
    void SetPlayerCurrency(int goldAmount);
    void UpdateWagonDashboard(string wagonName, int currentUpgrade, int maxUpgrade, int equippedSlots, int maxSlots, float currentLoad, float maxLoad);
    void UpdateWagonDisplay(Sprite wagonSprite);
    void ClearList();
    void AddBlueprintToList(WagonBlueprintData blueprint, bool isCraftable);
    void AddPartToList(PartData part, PartStatus status);
    void ShowBlueprintDetails(WagonBlueprintData blueprint, Dictionary<StableItemData, int> playerInventory);
    void ShowPartDetails(PartData part, Dictionary<StableItemData, int> playerInventory);
    void ShowEquippedPartDetails(PartData part);
    void ClearDetailPanel();
    void SetBlueprintSelection(WagonBlueprintData blueprint, bool isSelected);
    void SetPartSelection(PartData part, bool isSelected);
    void UpdateActionButton(bool interactable, string buttonText);
    void SetActiveMode(StableUIMode mode);
    void SetActiveFilter(UpgradeFilter filter);
}