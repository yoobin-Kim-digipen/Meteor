using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용
using System; // Action 사용

public class PartListItemUI : MonoBehaviour
{
    public TextMeshProUGUI partNameText;
    public TextMeshProUGUI statusText;
    public GameObject selectionIndicator; // 선택됐을 때 켤 오브젝트 (예: '>' 아이콘)

    private PartData associatedPartData;

    public void Setup(PartData part, PartStatus status, Action onClick)
    {
        partNameText.text = part.partName;
        GetComponent<Button>().onClick.AddListener(() => onClick());

        switch (status)
        {
            case PartStatus.Equipped:
                statusText.text = "<color=green>장착됨 ✓</color>";
                break;
            case PartStatus.Craftable:
                statusText.text = "<color=cyan>제작 가능</color>";
                break;
            case PartStatus.Locked:
                statusText.text = ""; // 혹은 "재료 부족"
                break;
        }
    }

    public void SetSelected(bool isSelected)
    {
        selectionIndicator.SetActive(isSelected);
    }
    public PartData GetPartData()
    {
        return associatedPartData;
    }
}