// 예시: LevelUpPopupManager.cs
using UnityEngine;
using TMPro;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;
    public TextMeshProUGUI popupText;

    public void ShowPopup(string msg, float duration = 2f)
    {
        popupPanel.SetActive(true);
        popupText.text = msg;
        StartCoroutine(HidePopupAfterDelay(duration));
    }

    private IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        popupPanel.SetActive(false);
    }
}
