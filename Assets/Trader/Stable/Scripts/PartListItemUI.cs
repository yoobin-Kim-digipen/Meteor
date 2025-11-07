using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PartListItemUI : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI partNameText;
    public TextMeshProUGUI statusText;
    public GameObject selectionIndicator;
    public Button itemButton;

    [Header("Appearance Settings")]
    public Color equippedColor = Color.green;
    public float disabledAlpha = 0.5f;

    private PartData associatedPartData;
    private Action onClickAction;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => onClickAction?.Invoke());
        }
    }

    public void Setup(PartData part, PartStatus status, Action onClick)
    {
        associatedPartData = part;
        onClickAction = onClick;
        partNameText.text = part.partName;

        // 버튼의 interactable 상태는 항상 true로 유지
        if (itemButton != null)
        {
            itemButton.interactable = true;
        }

        switch (status)
        {
            case PartStatus.Equipped:
                statusText.text = "장착됨";
                statusText.color = equippedColor;
                SetVisualState(true); // 시각적으로 활성화
                break;

            case PartStatus.Craftable:
                statusText.text = "";
                SetVisualState(true); // 시각적으로 활성화
                break;

            case PartStatus.Locked:
                statusText.text = "";
                SetVisualState(false); // 시각적으로만 비활성화 (흐리게)
                break;
        }
    }

    // 함수의 이름을 SetInteractable에서 SetVisualState로 변경하여 역할을 명확하게함.
    private void SetVisualState(bool isEnabled)
    {
        if (canvasGroup != null)
        {
            // Alpha 값만 조절하여 흐리게 만듭니다.
            canvasGroup.alpha = isEnabled ? 1.0f : disabledAlpha;
        }

        // itemButton.interactable = isEnabled; // 이 줄을 제거하거나 주석 처리
    }

    public void SetSelected(bool isSelected)
    {
        // 선택 표시는 항상 가능하도록 합니다.
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(isSelected);
        }
    }

    public PartData GetPartData()
    {
        return associatedPartData;
    }
}