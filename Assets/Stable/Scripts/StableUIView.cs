using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StableUIView : MonoBehaviour, IStableView
{
    [Header("A. 상단 정보")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI shardText;
    public Button exitButton;

    [Header("중앙 마차 디스플레이")]
    public Image wagonImage;

    [Header("B-1. 마차 스탯 및 목록")]
    public TextMeshProUGUI wagonNameText;
    public TextMeshProUGUI loadText;
    public TextMeshProUGUI slotText;
    public Transform partListContent;
    public GameObject partListItemPrefab;

    [Header("B-2. 상세 정보")]
    public Image selectedPartIcon;
    public TextMeshProUGUI selectedPartNameText;
    public TextMeshProUGUI selectedPartDescText;
    public Transform materialListContent;
    public GameObject materialListItemPrefab;

    [Header("하단 액션 버튼")]
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    private StablePresenter presenter;
    private List<PartListItemUI> currentPartListItems = new List<PartListItemUI>();

    // --- IStableView 인터페이스 이벤트 구현 ---
    public event PartSelectionHandler OnPartSelected;
    public event System.Action OnActionButtonClicked;
    public event System.Action OnExitClicked;

    void Awake()
    {
        presenter = new StablePresenter(this);

        actionButton.onClick.AddListener(() => OnActionButtonClicked?.Invoke());
        exitButton.onClick.AddListener(() => OnExitClicked?.Invoke());
    }

    void OnEnable()
    {
        StartCoroutine(InitializeUI());
    }

    private IEnumerator InitializeUI()
    {
        yield return null;
        presenter.OnViewEnabled();
    }

    // --- IStableView 인터페이스 메소드 구현 ---

    public void RefreshAll()
    {
        presenter.PopulateUI();
    }

    public void SetWagonStats(string wagonName, string load, string slot)
    {
        wagonNameText.text = wagonName;
        loadText.text = load;
        slotText.text = slot;
    }

    public void ClearPartList()
    {
        foreach (Transform child in partListContent)
        {
            Destroy(child.gameObject);
        }
        currentPartListItems.Clear();
    }

    public void AddPartToList(PartData part, PartStatus status)
    {
        GameObject itemGO = Instantiate(partListItemPrefab, partListContent);
        PartListItemUI itemUI = itemGO.GetComponent<PartListItemUI>();

        itemUI.Setup(part, status, () => OnPartSelected?.Invoke(part));
        currentPartListItems.Add(itemUI);
    }

    public void SetPartSelection(PartData part, bool isSelected)
    {
        PartListItemUI targetUI = currentPartListItems.Find(ui => ui.GetPartData() == part);
        if (targetUI != null)
        {
            targetUI.SetSelected(isSelected);
        }
    }

    public void ShowDetailPanel(PartData part, Dictionary<ItemData, int> playerInventory)
    {
        selectedPartIcon.sprite = part.icon;
        selectedPartNameText.text = part.partName;
        selectedPartDescText.text = part.description;

        foreach (Transform child in materialListContent)
        {
            Destroy(child.gameObject);
        }

        if (part.requiredMaterials == null) return;

        foreach (var material in part.requiredMaterials)
        {
            GameObject matGO = Instantiate(materialListItemPrefab, materialListContent);
            int ownedAmount = playerInventory.ContainsKey(material.item) ? playerInventory[material.item] : 0;
            matGO.GetComponent<MaterialListItemUI>().Setup(material, ownedAmount);
        }
    }

    public void UpdateActionButton(PartStatus status, bool interactable, string buttonText)
    {
        actionButton.interactable = interactable;
        actionButtonText.text = buttonText;
    }

    public void ClearDetailPanel()
    {
        selectedPartIcon.sprite = null;
        selectedPartIcon.color = new Color(1, 1, 1, 0); // 투명하게 처리
        selectedPartNameText.text = "부품을 선택하세요";
        selectedPartDescText.text = "";

        foreach (Transform child in materialListContent)
        {
            Destroy(child.gameObject);
        }

        actionButton.interactable = false;
        actionButtonText.text = "---";
    }
}