using UnityEngine;
using TMPro;
public class HeaderView : MonoBehaviour
{
    [Tooltip("플레이어의 재화를 표시할 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI goldText;

    public void UpdateCurrency(int goldAmount)
    {
        if (goldText != null)
        {
            // 통화 형식(e.g., 1,234)으로 변환하여 표시합니다.
            goldText.text = goldAmount.ToString("N0");
        }
    }
}