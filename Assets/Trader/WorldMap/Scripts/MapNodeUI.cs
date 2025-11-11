using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapNodeUI : MonoBehaviour
{
    private MapNodeData nodeData;

    public void Setup(MapNodeData data)
    {
        this.nodeData = data;
        GetComponentInChildren<TextMeshProUGUI>().text = data.nodeName;
        GetComponent<Button>().onClick.AddListener(HandleClick);
    }

    private void HandleClick()
    {
        Debug.Log(nodeData.nodeName + " 노드가 클릭됨! " + nodeData.sceneToLoad + " 씬으로 출발합니다!");
        // GameManager를 통해 씬 전환
        GameManager.Instance.StartCoroutine(
            GameManager.Instance.SwitchToOtherMapScene(nodeData.sceneToLoad)
        );
    }
}