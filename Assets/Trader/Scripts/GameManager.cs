using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("맵 생성 설정")]
    public RectTransform ParentTransform; // 맵 노드들의 부모 (에디터에서 할당)
    public GameObject mapNodePrefab;
    public int gridRows = 5;
    public int gridCols = 5;
    public Vector2 startPosition = new Vector2(-200, -200);
    public float nodeSpacing = 0f;
    // public Vector2 startPosition = new Vector2(-200, -200); // <-- 이 줄을 삭제하세요.

    [Range(0.1f, 1f)] // 10% ~ 100%
    public float gridHeightPercentage = 0.8f; // <-- 이 줄을 새로 추가하세요. (화면 세로의 80%를 차지)

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
    public GameObject TreasureTypeIcon;
    public string TreasureType;

    [Range(0f, 1f)] public float treasureChance = 0.2f; // 보물상자 출현 확률
    [Range(0f, 1f)] public float monsterChance = 0.3f;  // 몬스터 출현 확률
    [Range(0f, 1f)] public float wellChance = 0.1f;     // 샘 출현 확률

    [Header("UI 설정")]
    public TMP_Text nodeTypeDisplayText;  // 에디터에서 할당할 UI Text 컴포넌트
    public GameObject popupPanel;          // 팝업 패널 오브젝트 (에디터에서 할당)
    public TMP_Text popupMessageText;      // 팝업 내 메시지 텍스트
    public Button shopEnterButton;        // 상점 입장 버튼 (에디터에서 할당)
    public Button stableEnterButton;      // 마구간 입장 버튼 (에디터에서 할당)
    public GameObject videoPlayerPrefab;    // VideoPlayer 프리팹 (에디터에서 할당)
    private GameObject videoPlayerInstance; // 런타임에 생성되는 인스턴스
    private VideoPlayer videoPlayer;        // 실제 VideoPlayer 컴포넌트
    private System.Action onEnterAction;   // 입장 콜백
    public bool isVideoEnd = false;
    public bool isPopupActive = false;

    [Header("애니메이션 설정")]
    private Animator carriageAnimator; // 마차 프리팹에 있는 Animator 컴포넌트 참조

    private RectTransform gridParentRect; // 맵 노드들의 부모 (좌표 보정용)

    private MapNode[,] mapGrid;
    private bool[,] visitedNodes; // 방문 노드 기록

    private MapNode currentNode;
    private RectTransform currentCarriageRect;
    private bool isMoving = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        if (Instance == null)
        {
            var gameManagerPrefab = Resources.Load<GameObject>("GameManager");

            if (gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
                Debug.Log("GameManager가 씬 시작 전에 자동으로 생성되었습니다.");
            }
            else
            {
                Debug.LogError("자동 생성을 위한 'GameManager' 프리팹을 Resources 폴더에서 찾을 수 없습니다!");
            }
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // (선택 사항) 씬이 변경되어도 이 GameManager가 파괴되지 않게 함
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Debug.LogWarning("GameManager가 이미 존재하므로 새로 생성된 인스턴스를 파괴합니다.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "LocalMapScene")
        {
            if (mapGrid == null)
            {
                GameObject canvasObject = GameObject.Find("Canvas");

                if (canvasObject != null)
                {
                    ParentTransform = canvasObject.GetComponent<RectTransform>();
                }

                if (ParentTransform == null)
                {
                    Debug.LogError("'LocalMapScene'에서 'Canvas'를 찾지 못했습니다! 맵을 생성할 수 없습니다.");
                    return; // 맵 생성 중단
                }

                Debug.Log("LocalMapScene 로드 확인. 맵 생성을 시작합니다.");
                GenerateGridMap();
                SetInitialCarriagePosition();
            }
            else
            {
                Debug.Log("LocalMapScene 로드 확인. 맵이 이미 존재하므로 생성하지 않습니다.");
                if (ParentTransform == null)
                {
                    GameObject canvasObject = GameObject.Find("Canvas");
                    if (canvasObject != null) ParentTransform = canvasObject.GetComponent<RectTransform>();
                }

                UpdateFogOfWar();
            }
        }
        if (scene.name == "TreasureMapScene")
        {
            // 기존에 생성된 VideoPlayer 인스턴스가 있으면 제거
            if (videoPlayerInstance != null)
            {
                Destroy(videoPlayerInstance);
                videoPlayerInstance = null;
                videoPlayer = null;
            }

            // 보물상자 씬의 Canvas 찾기 및 ParentTransform 재설정
            GameObject canvasObject = GameObject.Find("Canvas");
            if (canvasObject != null)
                ParentTransform = canvasObject.GetComponent<RectTransform>();

            // 이후 프리팹 생성
            videoPlayerInstance = Instantiate(videoPlayerPrefab, ParentTransform);
            videoPlayer = videoPlayerInstance.GetComponent<VideoPlayer>();

            // VideoPlayer 컴포넌트 연결
            videoPlayer = videoPlayerInstance.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.prepareCompleted += OnVideoPrepared;
                videoPlayer.loopPointReached += OnVideoEnd;
                videoPlayer.Prepare();
            }
            else
            {
                Debug.LogError("VideoPlayer 컴포넌트를 프리팹에서 찾지 못했습니다.");
            }
        }
        else
        {
            if (videoPlayerInstance != null)
            {
                Destroy(videoPlayerInstance);
                videoPlayerInstance = null;
            }
            videoPlayer = null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void GenerateGridMap()
    {
        if (mapNodePrefab == null)
        {
            Debug.LogError("MapNodePrefab이 할당되지 않았습니다!");
            return;
        }
        if (ParentTransform == null)
        {
            Debug.LogError("ParentTransform이 할당되지 않았습니다! (1920x1080 캔버스 패널을 할당하세요)");
            return;
        }

        Rect parentRect = ParentTransform.rect;

        float totalGridHeight = parentRect.height * gridHeightPercentage;

        float nodeSize = (totalGridHeight - (nodeSpacing * (gridRows - 1))) / gridRows;

        float totalGridWidth = (nodeSize * gridCols) + (nodeSpacing * (gridCols - 1));

        float parentBottomLeftX = -parentRect.width * 0.5f;
        float parentBottomLeftY = -parentRect.height * 0.5f;

        float mapStartX = parentBottomLeftX + (parentRect.width - totalGridWidth) / 2;
        float mapStartY = parentBottomLeftY + (parentRect.height - totalGridHeight) / 2;


        mapGrid = new MapNode[gridRows, gridCols];
        visitedNodes = new bool[gridRows, gridCols];

        if (pathLineImage != null)
        {
            RectTransform lineInstance = Instantiate(pathLineImage, ParentTransform);
            lineInstance.name = "PathLine (Instance)"; // 하이어라키에서 알아보기 쉽게 이름 변경

            pathLineImage = lineInstance;

            pathLineImage.pivot = new Vector2(0f, 0.5f);
            pathLineImage.gameObject.SetActive(false);
        }

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < gridCols; col++)
            {
                GameObject nodeGO = Instantiate(mapNodePrefab, ParentTransform);
                nodeGO.name = $"MapNode_{row}_{col}";

                RectTransform nodeRect = nodeGO.GetComponent<RectTransform>();

                nodeRect.sizeDelta = new Vector2(nodeSize, nodeSize);

                float xPos = mapStartX + (nodeSize * 0.5f) + col * (nodeSize + nodeSpacing);
                float yPos = mapStartY + (nodeSize * 0.5f) + row * (nodeSize + nodeSpacing);

                nodeRect.anchoredPosition = new Vector2(xPos, yPos);

                GameObject fogGO = new GameObject("FogCover", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                fogGO.transform.SetParent(nodeGO.transform, false);
                RectTransform fogRect = fogGO.GetComponent<RectTransform>();
                fogRect.anchorMin = Vector2.zero;
                fogRect.anchorMax = Vector2.one;
                fogRect.offsetMin = Vector2.zero;
                fogRect.offsetMax = Vector2.zero;
                Image fogImage = fogGO.GetComponent<Image>();
                fogImage.color = new Color(0f, 0f, 0f, 0.9f);
                fogGO.SetActive(true);

                MapNode mapNode = nodeGO.GetComponent<MapNode>();
                if (mapNode != null)
                {
                    NodeType type = DetermineNodeType(row, col);
                    mapNode.Initialize(row, col, type);
                    mapGrid[row, col] = mapNode;
                    PlaceIconByNodeType(mapNode);
                    if (mapNode.nodeType == NodeType.Treasure)
                    {
                        if (mapNode.treasureType.HasValue)
                        {
                            TreasureTypeIcon = TreasureManager.Instance.GetTreasureIcon(mapNode.treasureType.Value);
                            TreasureType = TreasureManager.Instance.GetTreasureDescription(mapNode.treasureType.Value);
                        }
                    }
                }
            }
        }

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
        // 이동 중이면 절대 이동불가
        if (isMoving || isPopupActive)
        return false;

        // 팝업 패널이 열려 있다면 이동 불가
        if (popupPanel != null && popupPanel.activeInHierarchy)
            return false;

        if (targetRow < 0 || targetRow >= gridRows || targetCol < 0 || targetCol >= gridCols)
        {
            return false;
        }

        // 안개가 걷히지 않은 노드는 이동 불가
        if (!visitedNodes[targetRow, targetCol])
        {
            return false;
        }

        // 이동 허용 위치 체크 (오른쪽, 대각선 오른쪽 위, 위쪽)
        int rowDiff = targetRow - currentNode.row;
        int colDiff = targetCol - currentNode.col;

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
    public void ShowPopup(string message, System.Action enterAction, System.Action cancelAction = null)
    {
        if (popupPanel == null)
        {
            Debug.LogError("popupPanelPrefab이 GameManager에 할당되지 않았습니다!");
            return;
        }

        if (ParentTransform == null)
        {
            Debug.LogError("Canvas(ParentTransform)가 할당되지 않았습니다. 팝업을 생성할 수 없습니다.");
            return;
        }

        GameObject popupGO = Instantiate(popupPanel, ParentTransform);
        popupGO.transform.SetAsLastSibling();

        PopupPanel popupScript = popupGO.GetComponent<PopupPanel>();
        if (popupScript != null)
        {
            popupScript.Initialize(message, enterAction, cancelAction);
        }
        else
        {
            Debug.LogError("popupPanelPrefab에 PopupPanel.cs 스크립트가 없습니다!");
            Destroy(popupGO);
        }
        isPopupActive = true;  // 팝업 열림 표시
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
        UpdateFogOfWar();
    }

    public IEnumerator SwitchToOtherMapScene(string newScene)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        var local = SceneManager.GetSceneByName(currentScene.name);
        foreach (var go in local.GetRootGameObjects())
        {
            Debug.Log($"Deactivating GameObject: {go.name}");
            if (go.name == "GameManager") continue;
            go.SetActive(false);
        }
        if (SceneManager.GetSceneByName(newScene).isLoaded)
        {
            var newnew = SceneManager.GetSceneByName(newScene);
            foreach (var go in newnew.GetRootGameObjects())
            {
                Debug.Log($"Deactivating GameObject: {go.name}");
                if (go.name == "GameManager") continue;
                go.SetActive(true);
            }
            SceneManager.SetActiveScene(newnew);
        }
        else
        {
            yield return SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(newScene));
        }
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
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject != null)
            ParentTransform = canvasObject.GetComponent<RectTransform>();
        UpdateFogOfWar();
    }

    public void RestoreOldScene(string oldScene)
    {
        Scene oldS = SceneManager.GetSceneByName(oldScene);
        foreach (var go in oldS.GetRootGameObjects())
            go.SetActive(true);
        SceneManager.SetActiveScene(oldS);
    }

    public void UpdateFogOfWar()
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
        //UpdateFogOfWar()

        bool isFinalNode = currentNode.row == gridRows - 1 && currentNode.col == gridCols - 1;

        if (isFinalNode)
        {
            // 2. 마지막 노드가 맞다면, '스톤가드 마을'로 이동하는 씬 전환을 실행
            Debug.Log("타일맵의 끝에 도착! 목적지 마을로 이동합니다.");
            // "StoneguardVillageScene"은 실제 스톤가드 마을 씬의 이름으로 변경
            StartCoroutine(SwitchToOtherMapScene("StoneguardVillageScene"));
        }

        if (nodeTypeDisplayText != null)
        {
            // 2. 마지막 노드가 맞다면, '스톤가드 마을'로 이동하는 씬 전환을 실행
            Debug.Log("타일맵의 끝에 도착! 목적지 마을로 이동합니다.");
            // "StoneguardVillageScene"은 실제 스톤가드 마을 씬의 이름으로 변경
            StartCoroutine(SwitchToOtherMapScene("StoneguardVillageScene"));
        }

        var message = "";

        switch (currentNode.nodeType)
        {
            case NodeType.Battle:
                message = "배틀하시겠습니까?";
                break;
            default:
                message = "입장하시겠습니까?";
                break;
        }

        GameManager.Instance.ShowPopup(message,
            () =>
            {
                switch (currentNode.nodeType)
                {
                    case NodeType.Well:
                        StartCoroutine(SwitchToOtherMapScene("WellMapScene"));
                        break;
                    case NodeType.Battle:
                        StartCoroutine(SwitchToOtherMapScene("Battle_Sample_Scene"));
                        break;
                    case NodeType.Treasure:
                        StartCoroutine(SwitchToOtherMapScene("TreasureMapScene"));
                        break;
                    case NodeType.Empty:
                        StartCoroutine(SwitchToOtherMapScene("EmptyMapScene"));
                        break;
                }
                Debug.Log("노드 입장 처리!");
            },
            () =>
            {
                if (currentNode.nodeType == NodeType.Battle)
                {
                    CoinFlipManager.Instance.StartCoinFlip(
                        () =>
                        {
                            Debug.Log("코인이 앞면으로 나왔습니다!");
                            CoinFlipManager.Instance.flipUI.GetComponent<CoinFlipUI>().textUI.text = "성공";
                            UpdateFogOfWar();
                        },
                        () =>
                        {
                            Debug.Log("코인이 뒷면으로 나왔습니다!");
                            CoinFlipManager.Instance.flipUI.GetComponent<CoinFlipUI>().textUI.text = "실패";
                            GameManager.Instance.ShowPopup("한번 더 도전하시겠습니까?",
                                () =>
                                {
                                    CoinFlipManager.Instance.StartCoinFlip(
                                        () =>
                                        {
                                            Debug.Log("두번째 도전에서 코인이 앞면으로 나왔습니다!");
                                            CoinFlipManager.Instance.flipUI.GetComponent<CoinFlipUI>().textUI.text = "성공";
                                            UpdateFogOfWar();
                                        },
                                        () =>
                                        {
                                            Debug.Log("두번째 도전에서 코인이 뒷면으로 나왔습니다!");
                                            CoinFlipManager.Instance.flipUI.GetComponent<CoinFlipUI>().textUI.text = "실패";
                                            string sceneToLoad = (Random.value < 0.5f) ? "Battle_Sample_Scene" : "BattleMapScene";
                                            StartCoroutine(SwitchToOtherMapScene(sceneToLoad));
                                        }
                                    );
                                },
                                () =>
                                {
                                    string sceneToLoad = (Random.value < 0.5f) ? "Battle_Sample_Scene" : "BattleMapScene";
                                    StartCoroutine(SwitchToOtherMapScene(sceneToLoad));
                                }
                            );
                        }
                    );
                }
                else
                {
                    Debug.Log("노드 입장 취소!");
                    UpdateFogOfWar();
                }
            }
        );
        
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
        if (row == 0 && col == 0)
        {
            UpdateFogOfWar();
        }
        //UpdateFogOfWar();
    }


    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
        vp.prepareCompleted -= OnVideoPrepared;
    }

    public void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("비디오 재생 종료!");
        isVideoEnd = true;
    }

    public void SetvideoEnd(bool val)
    {
        isVideoEnd = val;
    }

}