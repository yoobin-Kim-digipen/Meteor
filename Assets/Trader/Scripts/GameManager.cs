using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("맵 생성 설정")]
    public RectTransform ParentTransform; // 맵 노드들의 부모 (에디터에서 할당)
    public GameObject mapNodePrefab;
    public int gridRows = 5;
    public int gridCols = 5;
    public float nodeSpacing = 0f;
    public Vector2 startPosition = new Vector2(-200, -200);

    [Header("마차 설정")]
    public GameObject carriagePrefab;
    public float carriageMoveSpeed = 0.5f;

    [Header("경로 표시 설정")]
    public RectTransform pathLineImage;
    public float pathLineThickness = 5f;

    [Header("아이콘 설정")]
    public GameObject treasureChestIconPrefab; // 보물상자 아이콘 프리팹
    public GameObject monsterIconPrefab;      // 몬스터 아이콘 프리팹
    public GameObject wellIconPrefab;         // 샘 아이콘 프리팹

    [Range(0f, 1f)] public float treasureChance = 0.2f; // 보물상자 출현 확률
    [Range(0f, 1f)] public float monsterChance = 0.3f;  // 몬스터 출현 확률
    [Range(0f, 1f)] public float wellChance = 0.1f;     // 샘 출현 확률

    [Header("UI 설정")]
    public TMP_Text nodeTypeDisplayText;  // 에디터에서 할당할 UI Text 컴포넌트
    public GameObject popupPanel;          // 팝업 패널 오브젝트 (에디터에서 할당)
    public TMP_Text popupMessageText;      // 팝업 내 메시지 텍스트
    private System.Action onEnterAction;   // 입장 콜백

    [Header("애니메이션 설정")]
    private Animator carriageAnimator; // 마차 프리팹에 있는 Animator 컴포넌트 참조

    private RectTransform gridParentRect; // 맵 노드들의 부모 (좌표 보정용)

    private MapNode[,] mapGrid;
    private bool[,] visitedNodes; // 방문 노드 기록

    private MapNode currentNode;
    private RectTransform currentCarriageRect;
    private bool isMoving = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // (선택 사항) 씬이 변경되어도 이 GameManager가 파괴되지 않게 함
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("GameManager가 이미 존재하므로 새로 생성된 인스턴스를 파괴합니다.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GenerateGridMap();
        SetInitialCarriagePosition();
    }

    void GenerateGridMap()
    {
        if (mapNodePrefab == null)
        {
            Debug.LogError("MapNodePrefab이 할당되지 않았습니다!");
            return;
        }

        mapGrid = new MapNode[gridRows, gridCols];
        visitedNodes = new bool[gridRows, gridCols];

        RectTransform prefabRect = mapNodePrefab.GetComponent<RectTransform>();
        float nodeWidth = prefabRect.rect.width;
        float nodeHeight = prefabRect.rect.height;

        GameObject gridParent = new GameObject("GridMapParent");
        gridParent.transform.SetParent(ParentTransform);
        gridParent.transform.localPosition = Vector3.zero;

        // pathLineImage 부모를 gridParent로 & pivot 왼쪽 중앙으로 설정
        if (pathLineImage != null)
        {
            pathLineImage.SetParent(gridParent.transform, false);
            pathLineImage.pivot = new Vector2(0f, 0.5f);
            pathLineImage.gameObject.SetActive(false);
        }

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                GameObject nodeGO = Instantiate(mapNodePrefab, gridParent.transform);
                nodeGO.name = $"MapNode_{row}_{col}";

                RectTransform nodeRect = nodeGO.GetComponent<RectTransform>();

                float xPos = startPosition.x + col * (nodeWidth + nodeSpacing);
                float yPos = startPosition.y + row * (nodeHeight + nodeSpacing);

                nodeRect.anchoredPosition = new Vector2(xPos, yPos);

                // 안개용 UI 이미지 동적 생성
                GameObject fogGO = new GameObject("FogCover", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fogGO.transform.SetParent(nodeGO.transform, false);
                RectTransform fogRect = fogGO.GetComponent<RectTransform>();
                fogRect.anchorMin = Vector2.zero;
                fogRect.anchorMax = Vector2.one;
                fogRect.offsetMin = Vector2.zero;
                fogRect.offsetMax = Vector2.zero;
                Image fogImage = fogGO.GetComponent<Image>();
                fogImage.color = new Color(0f, 0f, 0f, 0.9f); // 검은색 반투명
                fogGO.SetActive(true); // 기본적으로 안개 활성화

                MapNode mapNode = nodeGO.GetComponent<MapNode>();
                if (mapNode != null)
                {
                    // NodeType 결정: 확률에 의한 랜덤 타입 지정
                    NodeType type = DetermineNodeType(row, col);
                    mapNode.Initialize(row, col, type);
                    mapGrid[row, col] = mapNode;

                    // 아이콘 배치는 NodeType 기준으로 처리
                    PlaceIconByNodeType(mapNode);
                }
            }
        }

        // 시작 위치 노드는 방문 처리하여 안개 제거
        visitedNodes[0, 0] = true;
    }

    private NodeType DetermineNodeType(int row, int col)
    {
        if (row == 0 && col == 0)
            return NodeType.Empty; // 시작 노드는 비어있음

        float rand = Random.value;
        if (rand < monsterChance)
            return NodeType.Battle;
        else if (rand < monsterChance + treasureChance)
            return NodeType.Treasure;
        else if (rand < monsterChance + treasureChance + wellChance)
            return NodeType.Well;

        return NodeType.Empty;
    }

    private void PlaceIconByNodeType(MapNode node)
    {
        GameObject iconPrefab = null;
        string iconName = "";

        switch (node.nodeType)
        {
            case NodeType.Battle:
                iconPrefab = monsterIconPrefab;
                iconName = "MonsterIcon";
                break;
            case NodeType.Treasure:
                iconPrefab = treasureChestIconPrefab;
                iconName = "TreasureChestIcon";
                break;
            case NodeType.Well:
                iconPrefab = wellIconPrefab;
                iconName = "WellIcon";
                break;
        }

        if (iconPrefab != null)
        {
            GameObject iconGO = Instantiate(iconPrefab, node.transform);
            iconGO.name = iconName;
            RectTransform iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchoredPosition = Vector2.zero;
        }
    }

    void SetInitialCarriagePosition()
    {
        if (carriagePrefab == null)
        {
            Debug.LogError("CarriagePrefab이 할당되지 않았습니다!");
            return;
        }

        MapNode startNode = mapGrid[0, 0];

        if (startNode != null)
        {
            GameObject carriageGO = Instantiate(carriagePrefab, ParentTransform);
            carriageGO.name = "CarriageIcon";
            currentCarriageRect = carriageGO.GetComponent<RectTransform>();

            carriageAnimator = carriageGO.GetComponent<Animator>();
            if (carriageAnimator == null)
            {
                Debug.LogWarning("CarriagePrefab에 Animator 컴포넌트가 없습니다!");
            }

            currentNode = startNode;

            currentCarriageRect.position = startNode.GetComponent<RectTransform>().position;

            currentCarriageRect.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("시작 노드가 생성되지 않았습니다!");
        }
        UpdateFogOfWar(); // 시작 위치 안개 업데이트
        RevealAdjacentNodes(currentNode.row, currentNode.col); // 시작 위치 인접 노드 방문 처리
    }

    public void OnNodeSelected(MapNode selectedNode)
    {
        Debug.Log($"노드 클릭: ({selectedNode.row}, {selectedNode.col})");

        if (isMoving)
        {
            Debug.Log("마차가 이미 이동 중입니다!");
            return;
        }

        if (IsValidMove(selectedNode.row, selectedNode.col))
        {
            StartCoroutine(MoveCarriageSmoothly(selectedNode));
        }
        else
        {
            Debug.Log("이동할 수 없는 노드입니다!");
        }
    }

    public void OnNodeHoverEnter(MapNode hoveredNode)
    {
        if (isMoving || pathLineImage == null || !IsValidMove(hoveredNode.row, hoveredNode.col))
        {
            return;
        }

        Vector2 startPos = currentNode.GetComponent<RectTransform>().anchoredPosition;
        Vector2 endPos = hoveredNode.GetComponent<RectTransform>().anchoredPosition;

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        pathLineImage.anchoredPosition = startPos;
        pathLineImage.sizeDelta = new Vector2(distance, pathLineThickness);
        pathLineImage.localRotation = Quaternion.Euler(0, 0, angle);

        pathLineImage.SetAsLastSibling(); // 가장 위로 올림
        pathLineImage.gameObject.SetActive(true);
    }

    public void OnNodeHoverExit(MapNode hoveredNode)
    {
        if (pathLineImage != null)
        {
            pathLineImage.gameObject.SetActive(false);
        }
    }

    private bool IsValidMove(int targetRow, int targetCol)
    {
        if (targetRow < 0 || targetRow >= gridRows || targetCol < 0 || targetCol >= gridCols)
        {
            return false;
        }

        int rowDiff = targetRow - currentNode.row;
        int colDiff = targetCol - currentNode.col;

        // 오른쪽, 대각선 오른쪽 위, 위쪽으로만 이동 가능
        if (!((rowDiff == 0 && colDiff == 1) ||
              (rowDiff == 1 && colDiff == 1) ||
              (rowDiff == 1 && colDiff == 0)))
        {
            return false;
        }

        // 현재 위치 인접 3방향 노드가 모두 방문 상태여야 이동 가능
        int[][] adjacentNodes = new int[][]
        {
            new int[] {currentNode.row, currentNode.col + 1},      // 오른쪽
            new int[] {currentNode.row + 1, currentNode.col + 1},  // 오른쪽 위 대각선
            new int[] {currentNode.row + 1, currentNode.col}       // 위쪽
        };

        foreach (var pos in adjacentNodes)
        {
            int r = pos[0];
            int c = pos[1];
            if (r >= 0 && r < gridRows && c >= 0 && c < gridCols)
            {
                if (!visitedNodes[r, c])
                {
                    return false;
                }
            }
        }

        return true;
    }

    // 팝업을 띄우는 메서드
    public void ShowPopup(string message, System.Action enterAction)
    {
        if (popupMessageText != null)
            popupMessageText.text = message;
        if (popupPanel != null)
            popupPanel.SetActive(true);
        popupPanel.transform.SetAsLastSibling();
        onEnterAction = enterAction;
    }

    public void OnPopupEnter()
    {
        popupPanel.SetActive(false);
        onEnterAction?.Invoke(); ;
    }

    public void OnPopupCancel()
    {
        popupPanel.SetActive(false);
    }

    public IEnumerator SwitchScene(string oldScene, string newScene, bool keepOldScene = true)
    {
        if (keepOldScene)
        {
            Scene oldS = SceneManager.GetSceneByName(oldScene);
            foreach (var go in oldS.GetRootGameObjects())
                go.SetActive(false);
            yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
            Scene newS = SceneManager.GetSceneByName(newScene);
            SceneManager.SetActiveScene(newS);
        }
        else
        {
            yield return SceneManager.UnloadSceneAsync(oldScene);
            yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Single);
        }
    }

    public IEnumerator SwitchToOtherMapScene(string newScene)
    {
        var local = SceneManager.GetSceneByName("LocalMapScene");
        foreach (var go in local.GetRootGameObjects())
        {
            Debug.Log($"Deactivating GameObject: {go.name}");
            if (go.name == "GameManager") continue;
            go.SetActive(false);
        }
        yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(newScene));
    }

    public IEnumerator ReturnToLocalMapScene(string oldScene)
    {
        yield return SceneManager.UnloadSceneAsync(oldScene);
        var local = SceneManager.GetSceneByName("LocalMapScene");
        foreach (var go in local.GetRootGameObjects())
        {
            Debug.Log($"Activating GameObject: {go.name}");
            if (go.name == "GameManager") continue;
            go.SetActive(true);
        }
        SceneManager.SetActiveScene(local);
        UpdateFogOfWar();
    }

    public void RestoreOldScene(string oldScene)
    {
        Scene oldS = SceneManager.GetSceneByName(oldScene);
        foreach (var go in oldS.GetRootGameObjects())
            go.SetActive(true);
        SceneManager.SetActiveScene(oldS);
    }

    private IEnumerator MoveCarriageSmoothly(MapNode targetNode)
    {
        if (pathLineImage != null)
        {
            pathLineImage.gameObject.SetActive(false);
        }

        isMoving = true;
        Debug.Log($"마차 이동 시작: ({currentNode.row}, {currentNode.col}) -> ({targetNode.row}, {targetNode.col})");

        if (carriageAnimator != null)
        {
            carriageAnimator.SetBool("isWalking", true);
        }

        Vector3 startPos = currentCarriageRect.position;
        Vector3 endPos = targetNode.GetComponent<RectTransform>().position;
        float elapsedTime = 0f;

        Vector3 direction = endPos - startPos;
        if (direction.x < 0)
        {
            // 왼쪽으로 이동 시 마차 이미지 반전 또는 회전 설정 가능
        }
        else if (direction.x > 0)
        {
            // 오른쪽으로 이동 시 설정 가능
        }

        while (elapsedTime < carriageMoveSpeed)
        {
            currentCarriageRect.position = Vector3.Lerp(startPos, endPos, elapsedTime / carriageMoveSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentCarriageRect.position = endPos;
        currentNode = targetNode;
        visitedNodes[currentNode.row, currentNode.col] = true;

        RevealAdjacentNodes(currentNode.row, currentNode.col);
        //UpdateFogOfWar();

        if (nodeTypeDisplayText != null)
        {
            nodeTypeDisplayText.text = $"Node Property: {currentNode.nodeType}";
        }

        GameManager.Instance.ShowPopup("Would you like to enter?", () =>
        {
            switch (currentNode.nodeType)
            {
                case NodeType.Well:
                    StartCoroutine(SwitchToOtherMapScene("WellMapScene"));
                    break;
                case NodeType.Battle:
                    StartCoroutine(SwitchToOtherMapScene("BattleMapScene"));
                    break;
                case NodeType.Treasure:
                    StartCoroutine(SwitchToOtherMapScene("TreasureMapScene"));
                    break;
                case NodeType.Empty:
                    StartCoroutine(SwitchToOtherMapScene("EmptyMapScene"));
                    break;
            }
            Debug.Log("노드 입장 처리!");
        });

        isMoving = false;

        if (carriageAnimator != null)
        {
            carriageAnimator.SetBool("isWalking", false);
        }

        Debug.Log("마차 이동 완료!");
    }

    private void RevealAdjacentNodes(int row, int col)
    {
        int[][] adjacentNodes = new int[][]
        {
            new int[] {row, col + 1},      // 오른쪽
            new int[] {row + 1, col + 1},  // 오른쪽 위 대각선
            new int[] {row + 1, col}       // 위쪽
        };

        foreach (var pos in adjacentNodes)
        {
            int r = pos[0];
            int c = pos[1];
            if (r >= 0 && r < gridRows && c >= 0 && c < gridCols)
            {
                if (!visitedNodes[r, c])
                {
                    visitedNodes[r, c] = true;
                }
            }
        }
        if(row == 0 && col == 0)
        {
            UpdateFogOfWar();
        }
        //UpdateFogOfWar();
    }

    private void UpdateFogOfWar()
    {
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                MapNode node = mapGrid[row, col];
                if (node == null)
                    continue;

                Transform fogTransform = node.transform.Find("FogCover");
                if (fogTransform != null)
                {
                    bool isVisited = visitedNodes[row, col];
                    fogTransform.gameObject.SetActive(!isVisited);

                    // 아이콘 숨기기 처리
                    Transform monsterIcon = node.transform.Find("MonsterIcon");
                    Transform treasureIcon = node.transform.Find("TreasureChestIcon");
                    Transform wellIcon = node.transform.Find("WellIcon");

                    if (monsterIcon != null)
                        monsterIcon.gameObject.SetActive(isVisited);
                    if (treasureIcon != null)
                        treasureIcon.gameObject.SetActive(isVisited);
                    if (wellIcon != null)
                        wellIcon.gameObject.SetActive(isVisited);
                }
            }
        }
    }

}
