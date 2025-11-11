using UnityEngine;


public class Shop : MonoBehaviour
{
    public static Shop Instance { get; private set; }

    public enum ShopMode
    {
        None = 0,
        Buy,
        Sell
    }

    private ShopMode currentMode = ShopMode.None;

    //public Gold player_gold;

    [Header("Panels")]
    //구매모드에서 사용
    public GameObject shopPanel;
    public GameObject buyListPanel;

    //판매모드에서 사용
    public GameObject inventoryPanel;
    public GameObject sellListPalnel;

    private void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowBuyPanel();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowBuyPanel()
    {
        currentMode = ShopMode.Buy;

        shopPanel.SetActive(true);
        buyListPanel.SetActive(true);

        inventoryPanel.SetActive(false);
        sellListPalnel.SetActive(false);
    }

    public void ShowSellPanel()
    {
        currentMode = ShopMode.Sell;

        shopPanel.SetActive(false);
        buyListPanel.SetActive(false);

        inventoryPanel.SetActive(true);
        sellListPalnel.SetActive(true);
    }

    public ShopMode GetShopMode()
    {
        return currentMode;
    }
}
