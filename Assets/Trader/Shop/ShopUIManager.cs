using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIManager : MonoBehaviour
{
    public TradeUI tradeUI;

    //아이템 종류를 받아옴
    public Items items;
    //상점 종류
    
    //상점 이름
    public TextMeshProUGUI shop_name;
    //상점의 버튼 모음
    public GameObject ItemButtonPrefeb;
    //상점 종류에 따른 아이템
    List<ItemData> shop_items = new List<ItemData>();
    
    public List<ItemData> winterShop_items;
    //string[] item_list = new string[4];
    public Transform constentParent;

    void Start()
    {
        shop_items.Add(items.itemList[0]);
        shop_items.Add(items.itemList[1]);
        shop_items.Add(items.itemList[2]);

        //상점의 종류에 따라 상점의 이름이 바뀜
        shop_name.text = "winter shop";
        //상점의 종류에 따라 아이템 받아 올 수 있음

        foreach (ItemData item in shop_items)
        {
            GameObject itemObj = Instantiate(ItemButtonPrefeb, constentParent);
            Item_Button item_ButtonScript = itemObj.GetComponent<Item_Button>();
            item_ButtonScript.Set(item,tradeUI);
            
            Debug.Log("item button construct");
        }





    }


    void Update()
    {
        
    }
}
