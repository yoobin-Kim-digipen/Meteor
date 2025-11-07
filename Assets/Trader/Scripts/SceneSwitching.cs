using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitching : MonoBehaviour
{
    public void ReturnToLocalMapScene2(string oldScene)
    {
        var old = SceneManager.GetSceneByName(oldScene);
        foreach (var go in old.GetRootGameObjects())
        {
            Debug.Log($"Deactivating GameObject: {go.name}");
            if (go.name == "GameManager") continue;
            go.SetActive(false);
        }
        var local = SceneManager.GetSceneByName("LocalMapScene");
        foreach (var go in local.GetRootGameObjects())
        {
            Debug.Log($"Deactivating GameObject: {go.name}");
            if (go.name == "GameManager") continue;
            go.SetActive(true);
        }
        SceneManager.SetActiveScene(local);
        GameManager.Instance.UpdateFogOfWar();
    }
}
