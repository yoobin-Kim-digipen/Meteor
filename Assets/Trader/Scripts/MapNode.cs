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
    public NodeType nodeType;

    public TreasureType? treasureType = null; // Nullable로 보물 없는 상태 표현 가능

    // Initialize 메서드 수정
    public void Initialize(int r, int c, NodeType type)
    {
        row = r;
        col = c;
        nodeType = type;

        // Treasure 타입일 때만 보물 종류 할당
        if (type == NodeType.Treasure)
        {
            treasureType = TreasureManager.Instance.GetRandomTreasureType();
        }
        else
        {
            treasureType = null;
        }

        button = GetComponent<Button>();
        button.onClick.AddListener(OnNodeClicked);
    }

    private void OnNodeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNodeSelected(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNodeHoverEnter(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNodeHoverExit(this);
        }
    }
}
