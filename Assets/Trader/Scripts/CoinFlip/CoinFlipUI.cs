using TMPro;
using UnityEngine;

public class CoinFlipUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TextMeshProUGUI textUI;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        // 오브젝트가 활성화될 때 원하는 작업
        Debug.Log("오브젝트 활성화됨");
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때마다 text 초기화 등 작업
        textUI.text = "동전 뒤집기 중..";
        Debug.Log("오브젝트 비활성화됨, 텍스트 초기화");
    }
}
