// --- 파일명: UIManager.cs (수정된 버전) ---
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // --- 추가 ---
    // 어디서든 UIManager.Instance 라고 부르면 바로 이 스크립트를 찾아올 수 있게 해주는 '공용 주소' 같은 역할입니다.
    public static UIManager Instance;
    // -----------

    // 마을 기본 UI 그룹
    public GameObject villageUIGroup;

    // 마구간 패널
    public GameObject stablePanel;

    // --- 추가 ---
    void Awake()
    {
        // 만약 Instance가 아직 비어있다면, 자기 자신을 할당합니다.
        if (Instance == null)
        {
            Instance = this;
        }
        // 만약 Instance가 이미 채워져 있는데, 그게 내가 아니라면?
        // (다른 씬에서 넘어온 UIManager가 이미 있다는 뜻)
        else if (Instance != this)
        {
            // 나는 필요 없으므로 스스로를 파괴합니다.
            Destroy(gameObject);
        }
    }
    // -----------


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