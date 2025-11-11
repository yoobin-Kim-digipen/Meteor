using System.Collections.Generic;
using UnityEngine;

//아래 list에 ItemData를 추가하여 아이템 생성 가능.
public class Items : MonoBehaviour
{
    public static Items Instance { get; private set; }
    private void Awake()
    {
        Debug.Log("items 생성");
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


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
    }

    ~Items()
    {
        itemList.Clear();
    }


    public ItemData this[int id]
    {
        get
        {
            if (itemList[id-1].item_id == id )
            {
                return itemList[id-1];
            }
            else
            {
                Debug.Log("잘못된 데이터 접근");
                return null;
            }
        }

    }
}