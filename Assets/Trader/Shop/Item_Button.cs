using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item_Button : MonoBehaviour
{
    
    public TradeUI tradeUI;

    public Button mybutton;
    public Image Image;
    public ItemData itemdata;

    public TextMeshProUGUI Name;

    public TextMeshProUGUI Weight;

    public TextMeshProUGUI Price;
    
    void Start()
    {        


        mybutton.onClick.AddListener(OnButtonClicked);
        /*        if(Image ==  null)
                    Image = transform.Find("Image").GetComponent<Image>();
                if(Name == null)
                    Name = transform.Find("Item Name").GetComponent<TextMeshProUGUI>();*/
        Image.sprite = Resources.Load<Sprite>(itemdata.image_path);
        Name.text = itemdata.name;
        Weight.text = itemdata.weight.ToString();
        Price.text = itemdata.price.ToString() + "G";
    }

    void Update()
    {
    }
    public void OnButtonClicked()
    {
        tradeUI.Set_itemdata(itemdata);
        Debug.Log("¹öÆ° Å¬¸¯µÊ!");
    }

    public void Set(ItemData item, TradeUI ui)
    {
        itemdata = item;
        tradeUI = ui;
    }



}
