using UnityEngine;

[CreateAssetMenu(fileName = "New Map Node", menuName = "WorldMap/Map Node")]
public class MapNodeData : ScriptableObject
{
    public string nodeName = "새로운 장소";
    public string sceneToLoad = "LocalMapScene";

    [Tooltip("월드맵 UI에서의 상대적 위치 (0,0) = 좌하단, (1,1) = 우상단")]
    public Vector2 positionOnMap;
}