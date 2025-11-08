using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;


public class DetailPanelView : MonoBehaviour
{
    [Header("상세 정보 UI")]
    [Tooltip("선택된 아이템의 이미지를 표시할 UI Image")]
    [SerializeField] private Image selectedItemImage;
    [Tooltip("선택된 아이템의 이름을 표시할 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI selectedItemNameText;
    [Tooltip("선택된 아이템의 설명을 표시할 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI selectedItemDescText;

    [Header("필요 재료 목록")]
    [Tooltip("필요 재료 UI들이 생성될 부모 Transform")]
    [SerializeField] private Transform materialListContent;
    [Tooltip("필요 재료를 표시할 UI 프리팹 (MaterialListItemUI 포함)")]
    [SerializeField] private GameObject materialListItemPrefab;

    public void DisplayBlueprint(WagonBlueprintData blueprint, Dictionary<StableItemData, int> playerInventory)
    {
        if (blueprint == null)
        {
            Clear();
            return;
        }

        UpdateCommonInfo(blueprint.image, blueprint.wagonName, blueprint.description);
        PopulateMaterialList(blueprint.requiredMaterials, playerInventory);
    }

    public void DisplayPart(PartData part, Dictionary<StableItemData, int> playerInventory)
    {
        if (part == null)
        {
            Clear();
            return;
        }

        UpdateCommonInfo(part.image, part.partName, part.description);
        PopulateMaterialList(part.requiredMaterials, playerInventory);
    }

    public void DisplayEquippedPart(PartData part)
    {
        if (part == null)
        {
            Clear();
            return;
        }

        UpdateCommonInfo(part.image, part.partName, part.description);
        ClearMaterialList(); // 장착된 아이템은 재료 목록이 필요 없음
    }

    private void UpdateCommonInfo(Sprite sprite, string itemName, string description)
    {
        if (selectedItemImage != null)
        {
            selectedItemImage.sprite = sprite;
            selectedItemImage.color = (sprite == null) ? Color.clear : Color.white;
        }
        if (selectedItemNameText != null) selectedItemNameText.text = itemName;
        if (selectedItemDescText != null) selectedItemDescText.text = description;
    }
    private void PopulateMaterialList(List<RequiredMaterial> materials, Dictionary<StableItemData, int> playerInventory)
    {
        ClearMaterialList();
        if (materials == null || materialListItemPrefab == null) return;

        foreach (var material in materials)
        {
            GameObject matGO = Instantiate(materialListItemPrefab, materialListContent);

            // TryGetValue를 사용하여 더 안전하게 보유량 확인
            int ownedAmount = playerInventory.TryGetValue(material.item, out int amount) ? amount : 0;

            matGO.GetComponent<MaterialListItemUI>().Setup(material, ownedAmount);
        }
    }

    public void Clear()
    {
        UpdateCommonInfo(null, "항목을 선택하세요", "");
        ClearMaterialList();
    }

    private void ClearMaterialList()
    {
        if (materialListContent == null) return;

        foreach (Transform child in materialListContent)
        {
            Destroy(child.gameObject);
        }
    }
}