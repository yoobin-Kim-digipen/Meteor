using UnityEngine;
using TMPro;

public class LevelUpChoiceUI : MonoBehaviour
{
    public GameObject choicePanel;
    public TextMeshProUGUI[] optionTexts; // 버튼 설명용
    public UnityEngine.UI.Button[] optionButtons;

    public void ShowLevelUpChoices(string[] optionNames, System.Action<int> onChoose)
    {
        choicePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        for (int i = 0; i < 3; i++)
        {
            optionTexts[i].text = optionNames[i];
            int idx = i; // 캡처 문제 방지
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                choicePanel.SetActive(false);
                onChoose?.Invoke(idx);
            });
        }
    }
}
