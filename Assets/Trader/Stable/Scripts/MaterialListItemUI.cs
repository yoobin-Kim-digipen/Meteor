using UnityEngine;
using TMPro;

public class MaterialListItemUI : MonoBehaviour
{
    // 인스펙터에서 연결할 UI 요소들
    public TextMeshProUGUI materialNameText;
    public TextMeshProUGUI amountText;

    // 외부(StableUIController)에서 이 함수를 호출하여 내용을 채움
    public void Setup(RequiredMaterial required, int ownedAmount)
    {
        // 재료 이름 설정
        materialNameText.text = "- " + required.item.itemName; // 앞에 '-'를 붙여서 목록처럼 보이게 함

        // 보유량/필요량 텍스트 설정
        amountText.text = $"{ownedAmount} / {required.amount}";

        // 보유량이 부족하면 텍스트 색상을 빨간색으로 변경
        if (ownedAmount < required.amount)
        {
            amountText.color = Color.red;
        }
        else
        {
            amountText.color = Color.green; // 충분하면 흰색으로
        }
    }
}