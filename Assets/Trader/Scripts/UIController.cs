using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public GameObject popupPrefab1;   // Inspector에 할당
    public GameObject popupPrefab2;

    private GameObject popupInstance;
    private GameObject popupInstance2;

    private GameObject IconInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isVideoEnd && SceneManager.GetActiveScene().name == "TreasureMapScene")
        {
            ShowPopup();
            StartCoroutine(DelayedSceneChange(4f));
        }

    }

    public void ShowPopup()
    {
        // 씬에 존재하는 Canvas 오브젝트 찾기
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("씬에 Canvas 오브젝트가 없습니다!");
            return;
        }

        Transform canvasTransform = canvasObj.transform;

        // Canvas 오브젝트의 자식으로 팝업 인스턴스 생성
        popupInstance = Instantiate(popupPrefab1, canvasTransform);
        popupInstance2 = Instantiate(popupPrefab2, canvasTransform);
        Transform iconParent = popupInstance2.transform;
        Debug.Log("TreasureType: " + GameManager.Instance.TreasureType);
        IconInstance = Instantiate(GameManager.Instance.TreasureTypeIcon, iconParent);
        GameManager.Instance.SetvideoEnd(false);
        //StartCoroutine(GameManager.Instance.ReturnToLocalMapScene("TreasureMapScene"));
    }

    private IEnumerator DelayedSceneChange(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(popupInstance);
        Destroy(popupInstance2);
        Destroy(IconInstance);
        StartCoroutine(GameManager.Instance.ReturnToLocalMapScene("TreasureMapScene"));
    }
}
