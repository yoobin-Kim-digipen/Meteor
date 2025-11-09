using UnityEngine;

public class CoinFlipUIRegister : MonoBehaviour
{
    public GameObject flipUI;
    void OnEnable()
    {
        // 씬이 로드되고 이 UI가 생성될 때 매니저에 등록
        if (CoinFlipManager.Instance != null)
        {
            Debug.Log("등록 되었습니다.");
            CoinFlipManager.Instance.RegisterUI(flipUI);
        }
    }
}
