using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public RectTransform slidePanel;
    public float duration = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 화면 밖 오른쪽에서 안으로 슬라이드 인
    public IEnumerator SlideIn()
    {
        Vector2 startPos = new Vector2(Screen.width, 0);
        Vector2 endPos = Vector2.zero;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            slidePanel.anchoredPosition = Vector2.Lerp(startPos, endPos, 1 - t);
            yield return null;
        }
        slidePanel.anchoredPosition = endPos;
        slidePanel.gameObject.SetActive(false);
    }

    // 화면 안에서 왼쪽 밖으로 슬라이드 아웃
    public IEnumerator SlideOut()
    {
        slidePanel.gameObject.SetActive(true);
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = new Vector2(-Screen.width, 0);

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            slidePanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        slidePanel.anchoredPosition = endPos;
    }
}
