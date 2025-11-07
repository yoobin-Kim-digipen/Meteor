using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventSystem을 사용하기 위해 필요

public enum NodeType
{
    Empty,
    Well,
    Battle,
    Treasure
}

// 마우스 이벤트를 감지하기 위해 2개의 인터페이스를 추가
public class MapNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int row;
    public int col;

    private Button button;
    // private GameManager gameManager; // 1. GameManager 참조 변수 "제거"
    public NodeType nodeType;

    // 2. Initialize에서 GameManager 매개변수 "제거"
    public void Initialize(int r, int c, NodeType type)
    {
        row = r;
        col = c;
        // gameManager = manager; // "제거"
        nodeType = type;

        button = GetComponent<Button>();
        button.onClick.AddListener(OnNodeClicked);
    }

    private void OnNodeClicked()
    {
        // 3. gameManager 변수 대신 "GameManager.Instance"를 직접 사용
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNodeSelected(this);
        }
    }

    // 3. gameManager 변수 대신 "GameManager.Instance"를 직접 사용
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNodeHoverEnter(this);
        }
    }

    // 3. gameManager 변수 대신 "GameManager.Instance"를 직접 사용
    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNodeHoverExit(this);
        }
    }
}