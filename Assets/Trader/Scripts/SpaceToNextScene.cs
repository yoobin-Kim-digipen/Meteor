using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceToNextScene : MonoBehaviour
{
    void Update()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (Input.GetKeyDown(KeyCode.Space) && (currentScene.name != "LocalMapScene"))
        {
            StartCoroutine(GameManager.Instance.ReturnToLocalMapScene(currentScene.name));
            Debug.Log("스페이스바로 씬 변경됨!");
        }
    }
}