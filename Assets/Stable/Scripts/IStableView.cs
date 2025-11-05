using System.Collections.Generic;

public delegate void PartSelectionHandler(PartData part);

public interface IStableView
{
    void SetWagonStats(string wagonName, string load, string slot);
    void ClearPartList();
    void AddPartToList(PartData part, PartStatus status);
    void SetPartSelection(PartData part, bool isSelected);

    void ShowDetailPanel(PartData part, Dictionary<ItemData, int> playerInventory);
    void UpdateActionButton(PartStatus status, bool interactable, string buttonText);
    void ClearDetailPanel();
    void RefreshAll();

    event PartSelectionHandler OnPartSelected;
    event System.Action OnActionButtonClicked;
    event System.Action OnExitClicked;
}