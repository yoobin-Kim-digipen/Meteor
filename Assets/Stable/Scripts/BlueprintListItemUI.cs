using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BlueprintListItemUI : MonoBehaviour
{
    public TextMeshProUGUI blueprintNameText;
    public Button itemButton;
    public GameObject selectionIndicator;

    private WagonBlueprintData associatedBlueprintData;

    public void Setup(WagonBlueprintData blueprint, bool isCraftable, Action onClick)
    {
        associatedBlueprintData = blueprint;
        blueprintNameText.text = blueprint.wagonName;

        // TODO: isCraftable 상태에 따라 UI를 다르게 표시 (예: 텍스트 색상 변경)

        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => onClick?.Invoke());
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(isSelected);
        }
    }

    public WagonBlueprintData GetBlueprintData()
    {
        return associatedBlueprintData;
    }
}