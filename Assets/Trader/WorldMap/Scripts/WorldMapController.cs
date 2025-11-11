using UnityEngine;

public class WorldMapController : MonoBehaviour
{
    [Header("설정")]
    public MapNodeData[] nodesToPlace; // 인스펙터에서 배치할 노드 데이터들
    public GameObject nodePrefab;      // 노드로 사용할 버튼 프리팹

    void Start()
    {
        // 리스트에 있는 모든 노드 데이터에 대해 반복 실행
        foreach (var nodeData in nodesToPlace)
        {
            GameObject nodeGO = Instantiate(nodePrefab, transform);

            RectTransform nodeRect = nodeGO.GetComponent<RectTransform>();

            nodeRect.anchorMin = nodeData.positionOnMap;
            nodeRect.anchorMax = nodeData.positionOnMap;

            nodeRect.anchoredPosition = Vector2.zero;
            nodeGO.GetComponent<MapNodeUI>().Setup(nodeData);
        }
    }
}