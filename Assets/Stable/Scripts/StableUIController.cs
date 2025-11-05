using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용
using System.Collections.Generic;

public class StableUIController : MonoBehaviour
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
    public Transform partListContent; // 부품 목록이 생성될 ScrollView의 Content 오브젝트
    public GameObject partListItemPrefab; // 부품 목록 아이템으로 사용할 프리팹

    [Header("B-2. 상세 정보")]
    public Image selectedPartIcon;
    public TextMeshProUGUI selectedPartNameText;
    public TextMeshProUGUI selectedPartDescText;
    public Transform materialListContent; // 필요 재료 목록이 생성될 Content 오브젝트
    public GameObject materialListItemPrefab; // 필요 재료 아이템으로 사용할 프리팹

    [Header("하단 액션 버튼")]
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    // --- 내부 변수 ---
    private PlayerWagon currentWagon; // 현재 보고 있는 마차의 데이터
    private PartData selectedPart; // 좌측 목록에서 선택한 부품의 데이터
    private List<PartListItemUI> currentPartListItems = new List<PartListItemUI>(); // 화면에 생성된 목록 아이템들을 관리하기 위한 리스트

    // 이 UI 패널이 켜질 때마다 호출됩니다.
    void OnEnable()
    {
        // 1. StableManager에서 현재 플레이어의 마차 정보를 가져옵니다.
        //    (주의: StableManager와 Player 데이터가 준비되어 있어야 합니다)
        currentWagon = StableManager.Instance.GetCurrentViewingWagon();

        if (currentWagon == null)
        {
            Debug.LogError("표시할 마차가 없습니다! StableManager에서 플레이어 마차 데이터를 확인하세요.");
            return;
        }

        // 2. 가져온 마차 정보로 전체 UI를 업데이트합니다.
        UpdateAllUI();
    }

    // 전체 UI를 새로고침하는 함수
    public void UpdateAllUI()
    {
        UpdateWagonStatsUI(); // B-1 패널의 마차 스탯 업데이트
        PopulatePartList();   // B-1 패널의 부품 목록 채우기

        // 목록에 아이템이 있다면, 첫 번째 아이템을 자동으로 선택해서 보여줌
        if (currentPartListItems.Count > 0)
        {
            // 첫 번째 아이템의 PartData를 가져와서 SelectPart 함수 호출
            PartData firstPart = currentPartListItems[0].GetPartData();
            SelectPart(firstPart);
        }
        else
        {
            // 목록이 비어있으면 상세 정보 패널도 비움
            ClearDetailPanel();
        }
    }

    // B-1 패널의 마차 이름, 적재량, 운명 슬롯 UI를 업데이트
    void UpdateWagonStatsUI()
    {
        wagonNameText.text = currentWagon.baseData.wagonName;
        // TODO: 적재량은 기본 + 개조 효과를 합산해야 함. 지금은 임시로 기본값만 표시.
        loadText.text = $"적재량: {currentWagon.baseData.baseLoadCapacity} / {currentWagon.baseData.baseLoadCapacity}";

        slotText.text = $"운명 슬롯: {currentWagon.equippedDestinyParts.Count}/{currentWagon.baseData.destinyUpgradeSlots}";
    }
    // B-1 패널의 스크롤 목록을 모든 부품 정보로 채움
    void PopulatePartList()
    {
        // 1. 기존에 있던 목록 아이템들을 모두 삭제
        foreach (Transform child in partListContent)
        {
            Destroy(child.gameObject);
        }
        currentPartListItems.Clear();

        // 2. StableManager에서 게임에 존재하는 모든 부품 목록을 가져옴
        foreach (PartData part in StableManager.Instance.allAvailableParts)
        {
            // 3. 목록 아이템 프리팹을 생성
            GameObject itemGO = Instantiate(partListItemPrefab, partListContent);
            PartListItemUI itemUI = itemGO.GetComponent<PartListItemUI>();

            // 4. 이 부품의 현재 상태 (장착됨, 제작가능 등)를 StableManager에게 물어봄
            PartStatus status = StableManager.Instance.GetPartStatusForWagon(currentWagon, part);

            // 5. 프리팹에 정보를 설정하고, 클릭했을 때 'SelectPart' 함수가 호출되도록 연결
            itemUI.Setup(part, status, () => SelectPart(part));
            currentPartListItems.Add(itemUI);
        }
    }

    // 유저가 부품 목록에서 항목을 클릭했을 때 호출
    void SelectPart(PartData part)
    {
        selectedPart = part;

        // 1. 모든 아이템의 '선택됨' 표시를 일단 끈다.
        foreach (var item in currentPartListItems)
        {
            item.SetSelected(false);
        }
        // 2. 현재 클릭한 아이템에 해당하는 UI를 찾아서 '선택됨' 표시를 켠다.
        PartListItemUI targetUI = currentPartListItems.Find(ui => ui.GetPartData() == part);
        if (targetUI != null)
        {
            targetUI.SetSelected(true);
        }

        // 3. B-2 상세 정보 패널을 선택된 부품 정보로 업데이트
        UpdateDetailPanel();
    }

    // B-2 상세 정보 패널과 하단 액션 버튼을 업데이트
    void UpdateDetailPanel()
    {
        if (selectedPart == null) return;

        // 아이콘, 이름, 설명 업데이트
        selectedPartIcon.sprite = selectedPart.icon;
        selectedPartNameText.text = selectedPart.partName;
        selectedPartDescText.text = selectedPart.description;

        // 필요 재료 목록 업데이트
        foreach (Transform child in materialListContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var material in selectedPart.requiredMaterials)
        {
            // 1. 프리팹을 복제해서 생성
            GameObject matGO = Instantiate(materialListItemPrefab, materialListContent);

            // 2. 플레이어의 실제 재료 보유량 확인
            int ownedAmount = 0;
            if (StableManager.Instance.playerInventory.ContainsKey(material.item))
            {
                ownedAmount = StableManager.Instance.playerInventory[material.item];
            }

            // 3. 프리팹의 Setup 함수를 호출하여 내용 채우기
            matGO.GetComponent<MaterialListItemUI>().Setup(material, ownedAmount);
        }

        // 액션 버튼 상태 업데이트 (텍스트, 활성화 여부, 클릭 시 기능)
        UpdateActionButton();
    }

    // 액션 버튼을 업데이트하는 함수
    void UpdateActionButton()
    {
        PartStatus status = StableManager.Instance.GetPartStatusForWagon(currentWagon, selectedPart);
        actionButton.onClick.RemoveAllListeners();
        switch (status)
        {
            case PartStatus.Equipped:
                actionButtonText.text = "장착 해제";
                actionButton.interactable = true;
                //actionButton.onClick.AddListener(() => StableManager.Instance.UnequipPart(currentWagon, selectedPart));
                break;
            case PartStatus.Craftable:
                actionButtonText.text = "제작하기";
                actionButton.interactable = true;
                //actionButton.onClick.AddListener(() => StableManager.Instance.CraftPart(currentWagon, selectedPart));
                break;
            case PartStatus.Locked:
                actionButtonText.text = "제작 불가";
                actionButton.interactable = false;
                break;
        }
        // 제작/해제 후 UI가 새로고침되도록 리스너에 UpdateAllUI()를 추가해줄 수 있습니다.
        // 예: actionButton.onClick.AddListener(() => {
        //         StableManager.Instance.CraftPart(currentWagon, selectedPart);
        //         UpdateAllUI();
        //     });
    }

    // 상세 정보 패널을 비우는 함수
    void ClearDetailPanel()
    {
        selectedPartIcon.sprite = null; // 투명하게
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