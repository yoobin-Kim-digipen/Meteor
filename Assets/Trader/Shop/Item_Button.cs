using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item_Button : MonoBehaviour
{
    public enum Button_Mode
    {
        None = 0,
        ShopMode,
        BuyListMode,
        InventoryMode,
        SellListMode
    }
    public TradeUI tradeUI;

    public Button mybutton;
    public Image Image;
    public ItemData itemdata;

    public TextMeshProUGUI Name;

    public TextMeshProUGUI Weight;

    public TextMeshProUGUI Price;

    private int count = 1;
    private int pre_count = 1;
    private Button_Mode button_mode = Button_Mode.None;

    void Start()
    {        
        pre_count = count;
        mybutton.onClick.AddListener(OnButtonClicked);

        Image.sprite = Resources.Load<Sprite>(itemdata.image_path);
        if(count == 1)
        {
            Name.text = itemdata.name;
        }
        else
        {
            Name.text = itemdata.name + " x " + count;
        }
        Weight.text = "Weight : " + (itemdata.weight * count).ToString();
        Price.text = (itemdata.price * count).ToString() + "G";
    }

    void Update()
    {
        //count가 변화될시에만 text를 새로 출력
        if (pre_count != count)
        {
            if (count == 1)
            {
                Name.text = itemdata.name;
            }
            else
            {
                Name.text = itemdata.name + " x " + count;
            }
            Weight.text = "Weight : " + (itemdata.weight * count).ToString();
            Price.text = (itemdata.price * count).ToString() + "G";
            pre_count = count;
        }
    }
    public void OnButtonClicked()
    {
        switch(button_mode)
        {
            case Button_Mode.ShopMode:
                tradeUI.Set_TradeMode((int)button_mode);
                tradeUI.Set_itemdata(itemdata);
                break;
            case Button_Mode.BuyListMode:
                tradeUI.Set_TradeMode((int)button_mode);
                tradeUI.Set_itemdata(itemdata);
                tradeUI.Set_count(count);
                break;
            case Button_Mode.InventoryMode:
                tradeUI.Set_TradeMode((int)button_mode);
                tradeUI.Set_itemdata(itemdata);
                break;
            case Button_Mode.SellListMode:
                tradeUI.Set_TradeMode((int)button_mode);
                tradeUI.Set_itemdata(itemdata);
                tradeUI.Set_count(count);
                break;

        }

        Debug.Log("버튼 클릭됨!");
    }

    public void Set_ShopButton(ItemData item, TradeUI ui, int cnt = 1)
    {
        button_mode = Button_Mode.ShopMode;
        itemdata = item;
        tradeUI = ui;
        count = cnt;
    }

    public void Set_BuyListButton(ItemData item, TradeUI ui, int cnt = 1)
    {
        button_mode = Button_Mode.BuyListMode;
        itemdata = item;
        tradeUI = ui;
        count = cnt;
    }
    public void Set_InventroyButton(ItemData item, TradeUI ui, int cnt = 1)
    {
        button_mode = Button_Mode.InventoryMode;
        itemdata = item;
        tradeUI = ui;
        count = cnt;
    }
    public void Set_SellListButton(ItemData item, TradeUI ui, int cnt = 1)
    {
        button_mode = Button_Mode.SellListMode;
        itemdata = item;
        tradeUI = ui;
        count = cnt;
    }

    public void add_count(int cnt)
    {
        count += cnt;
        Debug.Log(count);
    }
    public void remove_count(int cnt)
    {
        count -= cnt;
        Debug.Log(count);
    }
    public void SetCount(int amount)
    {
        count = amount;
    }
}
