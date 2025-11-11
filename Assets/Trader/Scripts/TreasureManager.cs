using UnityEngine;
using System.Collections.Generic;

public enum TreasureType
{
    Gold, Gem, Potion
}

public class TreasureManager : MonoBehaviour
{
    public static TreasureManager Instance { get; private set; }

    private Dictionary<TreasureType, GameObject> treasureIcons = new Dictionary<TreasureType, GameObject>();
    private Dictionary<TreasureType, string> treasureDescriptions = new Dictionary<TreasureType, string>();

    [Header("보물별 아이콘")]
    public GameObject goldIcon;
    public GameObject gemIcon;
    public GameObject potionIcon;

    // 시작씬에서 자동 생성
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreateManager()
    {
        if (Instance == null)
        {
            var prefab = Resources.Load<GameObject>("TreasureManager");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("TreasureManager가 자동으로 생성되었습니다.");
            }
            else
            {
                Debug.LogError("'TreasureManager' 프리팹을 Resources 폴더에서 찾을 수 없습니다!");
            }
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        treasureIcons[TreasureType.Gold] = goldIcon;
        treasureIcons[TreasureType.Gem] = gemIcon;
        treasureIcons[TreasureType.Potion] = potionIcon;

        treasureDescriptions[TreasureType.Gold] = "반짝이는 금화";
        treasureDescriptions[TreasureType.Gem] = "아름다운 보석";
        treasureDescriptions[TreasureType.Potion] = "신비한 회복 포션";
    }

    public TreasureType GetRandomTreasureType()
    {
        float value = Random.value;
        if (value < 0.6f) return TreasureType.Gold;      // 60%
        else if (value < 0.9f) return TreasureType.Gem;  // 30%
        else return TreasureType.Potion;                 // 10%
    }

    public GameObject GetTreasureIcon(TreasureType type)
    {
        return treasureIcons.ContainsKey(type) ? treasureIcons[type] : null;
    }

    public string GetTreasureDescription(TreasureType type)
    {
        return treasureDescriptions.ContainsKey(type) ? treasureDescriptions[type] : "";
    }
}
