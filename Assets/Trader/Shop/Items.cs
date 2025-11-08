using System.Collections.Generic;
using UnityEngine;

//아래 list에 ItemData를 추가하여 아이템 생성 가능.
public class Items : MonoBehaviour
{
    public List<ItemData> itemList = new List<ItemData>()
    {
        new ItemData()
        {
            item_id = 1,
            image_path = "Item_img/stone",
            name = "stone",
            price = 5,
            weight = 5

        },
        new ItemData()
        {
            item_id = 2,
            image_path = "Item_img/log",
            name = "log",
            price = 20,
            weight = 20
        },
        new ItemData()
        {
            item_id = 3,
            image_path = "Item_img/apple",
            name = "apple",
            price = 7,
            weight = 2
        }
    };

    void Start()
    {
        Debug.Log("items 생성");
    }

    ~Items()
    {
        itemList.Clear();
    }
}