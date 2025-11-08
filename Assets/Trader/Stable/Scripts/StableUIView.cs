// --- 파일명: StableUIView.cs ---
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class StableUIView : MonoBehaviour, IStableView
{
    private StablePresenter presenter;

    [Header("하위 뷰 컴포넌트")]
    [SerializeField] private HeaderView headerView;
    [SerializeField] private WagonDashboardView dashboardView;
    [SerializeField] private DetailPanelView detailPanelView;

    [Header("모드/필터 탭")]
    [SerializeField] private Button craftModeButton;
    [SerializeField] private Button upgradeModeButton;
    [SerializeField] private CanvasGroup filterGroup;
    [SerializeField] private Button allFilterButton;
    [SerializeField] private Button basicFilterButton;
    [SerializeField] private Button destinyFilterButton;

    [Header("좌측 목록")]
    [SerializeField] private Transform listContentParent;
    [SerializeField] private GameObject blueprintListItemPrefab;
    [SerializeField] private GameObject partListItemPrefab;

    [Header("하단 액션 버튼")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;

    [Header("시각적 피드백 설정")]
    [SerializeField] private Color selectedTabColor = Color.yellow;
    [SerializeField] private Color deselectedTabColor = Color.white;
    [SerializeField] private Color selectedFilterColor = Color.cyan;
    [SerializeField] private Color deselectedFilterColor = Color.white;

    // 목록 아이템을 빠르게 찾기 위한 Dictionary
    private readonly Dictionary<WagonBlueprintData, BlueprintListItemUI> blueprintUIMap = new Dictionary<WagonBlueprintData, BlueprintListItemUI>();
    private readonly Dictionary<PartData, PartListItemUI> partUIMap = new Dictionary<PartData, PartListItemUI>();

    public event System.Action<StableUIMode> OnModeTabClicked;
    public event System.Action<UpgradeFilter> OnFilterClicked;
    public event BlueprintSelectionHandler OnBlueprintSelected;
    public event PartSelectionHandler OnPartSelected;
    public event System.Action OnActionButtonClicked;
    public event System.Action OnExitClicked;

    void Awake()
    {
        presenter = new StablePresenter(this);
        // 코드로 리스너를 추가/삭제하는 부분이 모두 사라져 매우 깔끔해집니다.
    }

    void OnEnable()
    {
        // UI가 활성화될 때 Presenter에게 알림
        presenter.OnViewEnabled();
    }

    // ----- 인스펙터의 OnClick()에 연결할 Public 함수들 -----
    public void OnCraftModeButtonClicked() => OnModeTabClicked?.Invoke(StableUIMode.Crafting);
    public void OnUpgradeModeButtonClicked() => OnModeTabClicked?.Invoke(StableUIMode.Upgrading);
    public void OnAllFilterButtonClicked() => OnFilterClicked?.Invoke(UpgradeFilter.All);
    public void OnBasicFilterButtonClicked() => OnFilterClicked?.Invoke(UpgradeFilter.Basic);
    public void OnDestinyFilterButtonClicked() => OnFilterClicked?.Invoke(UpgradeFilter.Destiny);
    public void OnActionButtonClickedInternal() => OnActionButtonClicked?.Invoke();
    public void OnExitButtonClickedInternal() => OnExitClicked?.Invoke();
    // ---------------------------------------------------

    #region Presenter로부터 오는 명령 구현
    public void SetPlayerCurrency(int goldAmount) => headerView?.UpdateCurrency(goldAmount);

    public void UpdateWagonDashboard(string wagonName, int currentUpgrade, int maxUpgrade, int equippedSlots, int maxSlots, float currentLoad, float maxLoad)
        => dashboardView?.UpdateDashboard(wagonName, currentUpgrade, maxUpgrade, equippedSlots, maxSlots, currentLoad, maxLoad);

    public void UpdateWagonDisplay(Sprite wagonSprite) => dashboardView?.UpdateWagonImage(wagonSprite);

    public void ClearList()
    {
        foreach (Transform child in listContentParent) Destroy(child.gameObject);
        blueprintUIMap.Clear();
        partUIMap.Clear();
    }

    public void AddBlueprintToList(WagonBlueprintData blueprint, bool isCraftable)
    {
        GameObject itemGO = Instantiate(blueprintListItemPrefab, listContentParent);
        var itemUI = itemGO.GetComponent<BlueprintListItemUI>();
        if (itemUI != null)
        {
            itemUI.Setup(blueprint, isCraftable, () => OnBlueprintSelected?.Invoke(blueprint));
            blueprintUIMap[blueprint] = itemUI;
        }
    }

    public void AddPartToList(PartData part, PartStatus status)
    {
        GameObject itemGO = Instantiate(partListItemPrefab, listContentParent);
        var itemUI = itemGO.GetComponent<PartListItemUI>();
        if (itemUI != null)
        {
            itemUI.Setup(part, status, () => OnPartSelected?.Invoke(part));
            partUIMap[part] = itemUI;
        }
    }

    public void ShowBlueprintDetails(WagonBlueprintData blueprint, Dictionary<StableItemData, int> playerInventory)
    {
        detailPanelView?.DisplayBlueprint(blueprint, playerInventory);
    }

    public void ShowPartDetails(PartData part, Dictionary<StableItemData, int> playerInventory)
    {
        detailPanelView?.DisplayPart(part, playerInventory);
    }

    public void ShowEquippedPartDetails(PartData part)
    {
        detailPanelView?.DisplayEquippedPart(part);
    }

    public void ClearDetailPanel()
    {
        detailPanelView?.Clear();
        UpdateActionButton(false, "---");
    }

    public void SetBlueprintSelection(WagonBlueprintData blueprint, bool isSelected)
    {
        if (blueprintUIMap.TryGetValue(blueprint, out var ui)) ui.SetSelected(isSelected);
    }

    public void SetPartSelection(PartData part, bool isSelected)
    {
        if (partUIMap.TryGetValue(part, out var ui)) ui.SetSelected(isSelected);
    }

    public void UpdateActionButton(bool interactable, string buttonText)
    {
        actionButton.interactable = interactable;
        actionButtonText.text = buttonText;
    }

    public void SetActiveMode(StableUIMode mode)
    {
        bool isUpgradingMode = (mode == StableUIMode.Upgrading);
        if (filterGroup != null)
        {
            filterGroup.alpha = isUpgradingMode ? 1f : 0f;
            filterGroup.interactable = isUpgradingMode;
            filterGroup.blocksRaycasts = isUpgradingMode;
        }
        craftModeButton.GetComponent<Image>().color = (mode == StableUIMode.Crafting) ? selectedTabColor : deselectedTabColor;
        upgradeModeButton.GetComponent<Image>().color = (mode == StableUIMode.Upgrading) ? selectedTabColor : deselectedTabColor;
    }

    public void SetActiveFilter(UpgradeFilter filter)
    {
        allFilterButton.GetComponent<Image>().color = (filter == UpgradeFilter.All) ? selectedFilterColor : deselectedFilterColor;
        basicFilterButton.GetComponent<Image>().color = (filter == UpgradeFilter.Basic) ? selectedFilterColor : deselectedFilterColor;
        destinyFilterButton.GetComponent<Image>().color = (filter == UpgradeFilter.Destiny) ? selectedFilterColor : deselectedFilterColor;
    }
    #endregion
}