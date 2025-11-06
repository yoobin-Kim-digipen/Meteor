using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WagonDashboardView : MonoBehaviour
{
    [Header("마차 디스플레이")]
    [Tooltip("마차의 이미지를 표시할 UI Image 컴포넌트")]
    [SerializeField] private Image wagonDisplayImage;
    [Tooltip("마차의 이름을 표시할 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI wagonNameText;

    [Header("마차 스탯 정보")]
    [Tooltip("기본 개조 레벨을 표시할 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI basicUpgradeText;
    [Tooltip("운명 슬롯 상태를 표시할 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI destinySlotsText;
    [Tooltip("적재량을 표시할 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI loadText;

    public void UpdateDashboard(string name, int currentUpgrade, int maxUpgrade, int equippedSlots, int maxSlots, float currentLoad, float maxLoad)
    {
        if (wagonNameText != null)
            wagonNameText.text = name;

        if (basicUpgradeText != null)
            basicUpgradeText.text = $"기본 개조: {currentUpgrade} / {maxUpgrade}";

        if (destinySlotsText != null)
            destinySlotsText.text = $"운명 슬롯: {equippedSlots} / {maxSlots}";

        if (loadText != null)
            loadText.text = $"적재량: {currentLoad} / {maxLoad}";
    }

    public void UpdateWagonImage(Sprite wagonSprite)
    {
        if (wagonDisplayImage == null) return;

        wagonDisplayImage.sprite = wagonSprite;
        // 스프라이트가 없으면 투명하게, 있으면 불투명하게 만듭니다.
        wagonDisplayImage.color = (wagonSprite == null) ? Color.clear : Color.white;
    }
}