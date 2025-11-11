using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // System.Action을 위해 필요

public class PopupPanel : MonoBehaviour
{
    [Header("팝업 내부 UI (프리팹에서 할당)")]
    public TMP_Text messageText;    // 팝업 메시지 텍스트
    public Button enterButton;      // 입장 버튼
    public Button cancelButton;     // 닫기 버튼 (이름은 달라도 됩니다)

    // GameManager가 호출할 콜백
    private System.Action onEnterCallback;
    private System.Action onCancelCallback;

    /// <summary>
    /// GameManager가 팝업을 생성한 직후 호출할 초기화 메서드
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    /// <param name="enterAction">"입장" 버튼 클릭 시 실행할 동작</param>
    public void Initialize(string message, System.Action enterAction, System.Action cancelAction = null)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        this.onEnterCallback = enterAction;
        this.onCancelCallback = cancelAction; // 추가

        if (enterButton != null)
        {
            enterButton.onClick.RemoveAllListeners();
            enterButton.onClick.AddListener(OnEnterClick);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelClick);
        }
    }

    /// <summary>
    /// "입장" 버튼 클릭 시 호출
    /// </summary>
    private void OnEnterClick()
    {
        // 저장해둔 콜백(동작)이 있다면 실행
        onEnterCallback?.Invoke();
        GameManager.Instance.isPopupActive = false;
        // 팝업 스스로를 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// "닫기" 또는 "취소" 버튼 클릭 시 호출
    /// </summary>
    private void OnCancelClick()
    {
        onCancelCallback?.Invoke();
        GameManager.Instance.isPopupActive = false;
        // 팝업 스스로를 파괴
        Destroy(gameObject);
    }
}