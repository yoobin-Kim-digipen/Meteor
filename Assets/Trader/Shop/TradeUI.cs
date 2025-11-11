using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Data;
using System;

public class TradeUI : MonoBehaviour
{
    public enum Trade_Mode
    {
        None,
        ShopMode,
        BuyListMode,
        InventoryMode,
        SellListMode
    }
    //Trade 패널의 콘텐츠 모음
    [SerializeField] private GameObject[] content;

    [Header("Image")]
    //아이템 이미지
    public Image Image;
    public TextMeshProUGUI Item_Name;

    [Header("Texts")]
    //Text 
    public TextMeshProUGUI purchase_count;
    public TextMeshProUGUI weight_num;
    public TextMeshProUGUI price_num;
    public TextMeshProUGUI totalGoldText;
    public TextMeshProUGUI totalWeightText;
    public TextMeshProUGUI FinalGoldText;

    [Header("Buttons")]
    //버튼모음
    public Button plus_button;
    public Button minus_button;
    public Button add_button;
    public Button bargin_button;
    public Button trade_button;

    //플레이어 골드
    public Gold player_gold;


    public ItemData itemdata;
    private ItemData previtemdata;

    //Trade 패널 변수
    private int items_counted = 1; // 몇개 살건지 - shop 모드에서 사용
    private int items_weight = 0;   // 아이템의 무게 총합
    private int items_price = 0;    // 아이템의 가격 총합

    //buy list
    public BuyList buyList;
    //sell list
    public SellList sellList;
    

    //동전던지기후 적용될 할인/증가 률
    private double GoldPersent = 0.7f;
    private Trade_Mode tradeMode = Trade_Mode.None;
    private Trade_Mode preMode = Trade_Mode.None;

    void Start()
    {

        plus_button.onClick.AddListener(Count_plus);
        minus_button.onClick.AddListener(Count_minus);
        add_button.onClick.AddListener(Add);
        trade_button.onClick.AddListener(Trade);
        bargin_button.onClick.AddListener(Bargin);
        //content.SetActive(false);
        foreach (var c in content)
        {
            c.SetActive(false);
        }
        Debug.Log(" 트레이드 UI 생성됨");
    }

    void Update()
    {
        switch (tradeMode)
        {
            case Trade_Mode.None:
                if(preMode != tradeMode)
                {
                    preMode = tradeMode;
                    Debug.Log("선택된 아이템 없어짐");
                    foreach (var c in content)
                    {
                        c.SetActive(false);

                    }
                    previtemdata = itemdata;
                }
                break;
            case Trade_Mode.ShopMode:
                if (preMode != tradeMode)
                {
                    items_counted = 1;
                    preMode = tradeMode;
                    foreach (var c in content)
                    {
                        c.SetActive(true);
                    }
                }

                if (itemdata != previtemdata)
                {
                    previtemdata = itemdata; // 현재 상태 저장
                    if (itemdata == null)
                    {
                        Debug.Log("선택된 아이템 없음");
                    }
                    else
                    {
                        Debug.Log("선택된 아이템 있음");
                        trade_reset();
                        Image.sprite = Resources.Load<Sprite>(itemdata.image_path);
                        Item_Name.text = itemdata.name;
                    }
                }
                break;
            case Trade_Mode.BuyListMode:
                if(preMode != tradeMode)
                {
                    Debug.Log("모드가 " + preMode + "에서 " + tradeMode +"로 변경됨");
                    preMode = tradeMode;
                    foreach (var c in content)
                    {
                        c.SetActive(true);
                    }
                    content[9].SetActive(false);
                }


                if (itemdata != previtemdata)
                {
                    previtemdata = itemdata; // 현재 상태 저장

                    if (itemdata == null)
                    {
                        Debug.Log("선택된 아이템 없음");
                    }
                    else
                    {
                        Debug.Log("선택된 아이템 있음");

                        Image.sprite = Resources.Load<Sprite>(itemdata.image_path);
                        Item_Name.text = itemdata.name;
                    }
                }
                break;
            case Trade_Mode.InventoryMode:
                if (preMode != tradeMode)
                {
                    Debug.Log("모드가 " + preMode + "에서 " + tradeMode + "로 변경됨");
                    items_counted = 1;
                    preMode = tradeMode;
                    foreach (var c in content)
                    {
                        c.SetActive(true);
                    }
                }

                if (itemdata != previtemdata)
                {
                    previtemdata = itemdata; // 현재 상태 저장

                    if (itemdata == null)
                    {
                        Debug.Log("선택된 아이템 없음");
                    }
                    else
                    {
                        Debug.Log("선택된 아이템 있음");
                        trade_reset();
                        Image.sprite = Resources.Load<Sprite>(itemdata.image_path);
                        Item_Name.text = itemdata.name;
                    }
                }
                break;
            case Trade_Mode.SellListMode:
                if (preMode != tradeMode)
                {
                    Debug.Log("모드가 " + preMode + "에서 " + tradeMode + "로 변경됨");
                    preMode = tradeMode;
                    foreach (var c in content)
                    {
                        c.SetActive(true);
                    }
                    content[9].SetActive(false);
                }

                if (itemdata != previtemdata)
                {
                    previtemdata = itemdata; // 현재 상태 저장

                    if (itemdata == null)
                    {
                        Debug.Log("선택된 아이템 없음");
                    }
                    else
                    {
                        Debug.Log("선택된 아이템 있음");

                        Image.sprite = Resources.Load<Sprite>(itemdata.image_path);
                        Item_Name.text = itemdata.name;
                    }
                }
                break;
        }

  
        if (itemdata != null)
        {
            items_weight = itemdata.weight * items_counted;
            items_price = itemdata.price * items_counted;

            purchase_count.text = items_counted.ToString();
            weight_num.text = items_weight.ToString();
            price_num.text = items_price.ToString() + "G";
            if (Shop.Instance.GetShopMode() == Shop.ShopMode.Buy)
            {
                totalGoldText.text = "Buy List's Total Gold : " + buyList.Get_totalgold();
                totalWeightText.text = "Buy List's Total Weight : " + buyList.Get_totalweight();
            }
            else if(Shop.Instance.GetShopMode() == Shop.ShopMode.Sell)
            {
                totalGoldText.text = "Sell List's Total Gold : " + sellList.Get_totalgold();
                totalWeightText.text = "Sell List's Total Weight : " + sellList.Get_totalweight();
            }
        }

        
    }

    public void Set_TradeMode(int i)
    {
        if(i != (int)tradeMode )
        {
            tradeMode = (Trade_Mode)i;
            Debug.Log("트레이드 모드가 '" + tradeMode + "' 로 변경됨");

        }
       
    }

    public int GetTradeMode() 
    {
        return (int)tradeMode;
    }

    public void Set_count(int count)
    {
        items_counted = count; 
    }

    public void Set_itemdata(ItemData item)
    {

        Debug.Log("아이템 선택됨");
        itemdata = item;
    }

    public void Count_plus()
    {
        switch(tradeMode)
        {
            case Trade_Mode.ShopMode:
                items_counted++;
                break;
            case Trade_Mode.BuyListMode:
                buyList.AddList(itemdata.item_id, 1);
                items_counted++;
                break;
            case Trade_Mode.InventoryMode:
                items_counted++;
                break;
            case Trade_Mode.SellListMode:
                sellList.AddList(itemdata.item_id, 1);
                items_counted++;
                break;
        }
    }

    public void Count_minus()
    {
        switch (tradeMode)
        {
            case Trade_Mode.ShopMode:
                Debug.Log("샵 1개감소");

                if (items_counted <= 1)
                    return;
                items_counted--;
                break;
            case Trade_Mode.BuyListMode:
                buyList.RemoveList(itemdata.item_id, 1);
                items_counted--;
                Debug.Log("버이리스트 1개감소");

                if (items_counted <= 0)
                {
                    Debug.Log("BuyList안 아이템 없어짐");
                    HidePanel();
                    itemdata = null;
                }
                break;
            case Trade_Mode.InventoryMode:
                if (items_counted <= 1)
                    return;
                items_counted--;
                break;
            case Trade_Mode.SellListMode:
                sellList.RemoveList(itemdata.item_id, 1);
                items_counted--;
                Debug.Log("셀리스트 1개감소");

                if (items_counted <= 0)
                {
                    Debug.Log("셀리스트안 아이템 없어짐");
                    HidePanel();
                    itemdata = null;
                }
                break;
        }

    }
    public void Add()
    {
        if(Shop.Instance.GetShopMode() == Shop.ShopMode.Sell)
        {
            sellList.AddList(itemdata.item_id, items_counted);
        }
        else if(Shop.Instance.GetShopMode() == Shop.ShopMode.Buy)
        {
            buyList.AddList(itemdata.item_id, items_counted);
        }
    }


    public void Trade()
    {
        if (Shop.Instance.GetShopMode() == Shop.ShopMode.Sell)
        {
            if (sellList.Get_totalweight() <= 0)
            {
                Debug.Log("아이템을 선택해 주세요");
                return;
            }
            player_gold.Add_Gold(sellList.Get_totalgold());
            sellList.Sell_items();
            tradeMode = Trade_Mode.None;

        }
        else if (Shop.Instance.GetShopMode() == Shop.ShopMode.Buy)
        {
            if (buyList.Get_totalweight() <= 0)
            {
                Debug.Log("아이템을 선택해 주세요");
                return;
            }
            if (Inventory.Instance.GetAvailableWeight() >= buyList.Get_totalweight())
            {
                if (player_gold.Use_gold(buyList.Get_totalgold()))
                {

                    buyList.Buy_items();
                    tradeMode = Trade_Mode.None;
                    Debug.Log("거래성공~");
                }
                else
                {
                    Debug.Log("거래실패~");
                }
            }
            else
            {
                Debug.Log("용량이 부족합니다!");
            }
        }


    }

    public void Bargin()
    {
        if(items_price > 0)
        {
            CoinFlipManager.Instance.StartCoinFlip(
                () => {
                    Debug.Log("코인이 앞면으로 나왔습니다!");
                    items_price = (int)((float)items_price * 0.7f);
                    CoinFlipManager.Instance.flipUI.GetComponent<CoinFlipUI>().textUI.text = "SUCCES " + items_price.ToString();
                },
                () => {
                    Debug.Log("코인이 뒷면으로 나왔습니다!");
                    CoinFlipManager.Instance.flipUI.GetComponent<CoinFlipUI>().textUI.text = "FAILED " + items_price.ToString();
                }
            );
        }
        

    }


    public void HidePanel()
    {
        foreach (var c in content)
        {
            c.SetActive(false);
        }
        tradeMode = Trade_Mode.None;
    }

    public void trade_reset()
    {
        Debug.Log("트레이드 창 값 리셋됨");
        items_counted = 1;
        items_price = 0;
        items_weight = 0;
    }
}
