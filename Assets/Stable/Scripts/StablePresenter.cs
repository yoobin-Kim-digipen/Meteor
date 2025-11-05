using System.Collections.Generic;
public class StablePresenter
{
    private IStableView view;
    private PlayerWagon currentWagon;
    private PartData selectedPart;

    // 생성자: View와 연결
    public StablePresenter(IStableView view)
    {
        this.view = view;
        ConnectEvents();
    }

    // View에서 발생하는 이벤트를 Presenter의 함수와 연결합니다.
    private void ConnectEvents()
    {
        view.OnPartSelected += HandlePartSelection;
        view.OnActionButtonClicked += HandleActionButtonClick;
        // view.OnExitClicked += HandleExit; // TODO: 나가기 버튼 로직 구현 시 연결
    }

    // UI가 활성화될 때 View에 의해 호출됩니다.
    public void OnViewEnabled()
    {
        // Model에서 데이터를 가져옵니다.
        currentWagon = StableManager.Instance.GetCurrentViewingWagon();
        if (currentWagon == null)
        {
            UnityEngine.Debug.LogError("Presenter: 표시할 마차 데이터를 찾을 수 없습니다.");
            return;
        }

        // View에 전체 UI를 새로 그리라고 명령합니다.
        view.RefreshAll();
    }

    // View가 UI를 채워야 할 때 호출하는 함수입니다. Presenter가 필요한 데이터를 제공합니다.
    public void PopulateUI()
    {
        // 1. 마차 스탯 정보 가공 및 전달
        string load = $"적재량: {currentWagon.baseData.baseLoadCapacity} / {currentWagon.baseData.baseLoadCapacity}";
        string slot = $"운명 슬롯: {currentWagon.equippedDestinyParts.Count}/{currentWagon.baseData.destinyUpgradeSlots}";
        view.SetWagonStats(currentWagon.baseData.wagonName, load, slot);

        // 2. 부품 목록 정보 가공 및 전달
        view.ClearPartList();
        var allParts = StableManager.Instance.allAvailableParts;
        foreach (PartData part in allParts)
        {
            PartStatus status = StableManager.Instance.GetPartStatusForWagon(currentWagon, part);
            view.AddPartToList(part, status);
        }

        // 3. 목록의 첫 번째 부품을 자동으로 선택하도록 처리
        if (allParts.Count > 0)
        {
            HandlePartSelection(allParts[0]);
        }
        else
        {
            view.ClearDetailPanel();
        }
    }

    // View에서 부품 선택 이벤트가 발생했을 때 처리하는 로직
    private void HandlePartSelection(PartData part)
    {
        // 이전에 선택된 아이템이 있었다면, View에게 선택 해제 표시를 하라고 명령
        if (selectedPart != null)
        {
            view.SetPartSelection(selectedPart, false);
        }

        selectedPart = part;

        // View에게 새로 선택된 아이템에 선택 표시를 하고, 상세 정보를 보여주라고 명령
        view.SetPartSelection(selectedPart, true);
        view.ShowDetailPanel(selectedPart, StableManager.Instance.playerInventory);

        // 액션 버튼 상태 업데이트 명령
        UpdateActionButtonState();
    }

    // View에서 액션 버튼 클릭 이벤트가 발생했을 때 처리하는 로직
    private void HandleActionButtonClick()
    {
        if (selectedPart == null) return;

        PartStatus status = StableManager.Instance.GetPartStatusForWagon(currentWagon, selectedPart);

        // 실제 로직 실행은 Model(StableManager)에게 위임합니다.
        // TODO: 아래 주석 처리된 부분을 실제 함수로 구현/연결해야 합니다.
        if (status == PartStatus.Equipped)
        {
            // StableManager.Instance.UnequipPart(currentWagon, selectedPart);
            UnityEngine.Debug.Log($"{selectedPart.partName} 장착 해제 시도");
        }
        else if (status == PartStatus.Craftable)
        {
            // StableManager.Instance.CraftPart(currentWagon, selectedPart);
            UnityEngine.Debug.Log($"{selectedPart.partName} 제작 시도");
        }

        // 로직 실행 후, 데이터가 변경되었을 수 있으므로 UI를 다시 그리라고 명령합니다.
        view.RefreshAll();
    }

    // 액션 버튼의 상태를 결정하고 View에게 업데이트하라고 명령합니다.
    private void UpdateActionButtonState()
    {
        if (selectedPart == null) return;

        PartStatus status = StableManager.Instance.GetPartStatusForWagon(currentWagon, selectedPart);
        switch (status)
        {
            case PartStatus.Equipped:
                view.UpdateActionButton(status, true, "장착 해제");
                break;
            case PartStatus.Craftable:
                view.UpdateActionButton(status, true, "제작하기");
                break;
            case PartStatus.Locked:
                view.UpdateActionButton(status, false, "제작 불가");
                break;
        }
    }
}