using System.Collections.Generic;
using UnityEngine;
using System;

public class BuyList : MonoBehaviour
{
    public TradeUI tradeUI;

    public GameObject ItemButtonPrefeb;
    //리스트가 생성될 곳
    public Transform constentParent;

    //구매목록 버튼
    private Dictionary<int, GameObject> button_objectList = new Dictionary<int, GameObject>();
    private Dictionary<int, Item_Button> buttonList = new Dictionary<int,Item_Button>();
    //구매목록
    private Dictionary<int,int> buyList = new Dictionary<int, int>();


    private int total_gold = 0;
    private int total_weight = 0;
    public event Action OnBuyListChanged;
    void Start()
    {
        OnBuyListChanged += Calculate_data;
        Debug.Log("BuyList 생성");
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void OnDestroy()
    {
        OnBuyListChanged -= Calculate_data;
    }
    public int Get_totalgold()
    {
        return total_gold;
    }
    public int Get_totalweight()
    {
        return total_weight;
    }

    public void Calculate_data()
    {
        total_gold = 0;
        total_weight = 0;
        foreach (int i in buyList.Keys)
        {
            total_gold += Items.Instance[i].price * buyList[i];
            total_weight += Items.Instance[i].weight * buyList[i];
        }

        Debug.Log("BuyList's Total gold = " + total_gold);
        Debug.Log("BuyList's Total weight = " + total_weight);

    }
    public void AddList(int item_id, int count)
    {

        if (buyList.ContainsKey(item_id))
        {
            buyList[item_id] += count;
            buttonList[item_id].add_count(count);

            Debug.Log("아이템 번호" + item_id + "이 " + count + "만큼 추가됨 총: " + buyList[item_id]);

        }
        else
        {
            buyList.Add(item_id, count);
            GameObject itemObj = Instantiate(ItemButtonPrefeb, constentParent);
            Item_Button item_ButtonScript = itemObj.GetComponent<Item_Button>();
            item_ButtonScript.Set_BuyListButton(Items.Instance[item_id], tradeUI, count);
            button_objectList.Add(item_id, itemObj);
            buttonList.Add(item_id, item_ButtonScript);
            Debug.Log("아이템 번호" + item_id + "가 " + count + "만큼 새로 추가됨 총: " + buyList[item_id]);

        }
        OnBuyListChanged?.Invoke();
    }

    public void RemoveList(int item_id, int count)
    {
        if (buyList.ContainsKey(item_id))
        {
            buyList[item_id] -= count;
            buttonList[item_id].remove_count(count);

            Debug.Log("아이템 번호" + item_id + "이" + count + "만큼 감소됨");
            if (buyList[item_id] <= 0)
            {
                Destroy(buttonList[item_id]);
                Destroy(button_objectList[item_id]);
                buttonList.Remove(item_id);
                button_objectList.Remove(item_id);
                buyList.Remove(item_id);
                Debug.Log("아이템 리스트 삭제");
            }
            OnBuyListChanged?.Invoke();
        }
        else
        {
            Debug.Log("잘못된 데이터 접근");
        }
    }

    public void Buy_items()
    {
        foreach (int item_id in buyList.Keys)
        {
            Inventory.Instance.AddItem(item_id, buyList[item_id]);
        }
        ResetList();
    }
    public void ResetList()
    {
        foreach(int item_id in buttonList.Keys)
        {
            Destroy(buttonList[item_id]);
            Destroy(button_objectList[item_id]);
        }

        buttonList.Clear();
        button_objectList.Clear();
        buyList.Clear();

        OnBuyListChanged?.Invoke();
    }

}
