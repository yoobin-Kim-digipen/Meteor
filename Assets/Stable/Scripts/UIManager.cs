using UnityEngine;

public class UIManager : MonoBehaviour
{
    // 마을 기본 UI 그룹
    public GameObject villageUIGroup;

    // 마구간 패널
    public GameObject stablePanel;

    // 마구간 UI를 여는 함수
    public void OpenStablePanel()
    {
        villageUIGroup.SetActive(false); // 마을 UI를 끈다
        stablePanel.SetActive(true);     // 마구간 UI를 켠다
    }

    // 마구간 UI를 닫는 함수
    public void CloseStablePanel()
    {
        stablePanel.SetActive(false);   // 마구간 UI를 끈다
        villageUIGroup.SetActive(true);    // 마을 UI를 켠다
    }
}