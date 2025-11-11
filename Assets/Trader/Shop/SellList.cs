using System.Collections.Generic;
using UnityEngine;
using System;

public class SellList : MonoBehaviour
{
    public TradeUI tradeUI;

    public GameObject ItemButtonPrefeb;
    public Transform constentParent;
    //판매목록 버튼
    private Dictionary<int, GameObject> button_objectList = new Dictionary<int, GameObject>();
    private Dictionary<int, Item_Button> buttonList = new Dictionary<int, Item_Button>();

    //판매목록
    private Dictionary<int, int> sellList = new Dictionary<int, int>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private int total_gold = 0;
    private int total_weight = 0;
    public event Action OnSellListChanged;
    void Start()
    {
        OnSellListChanged += Calculate_data;
        Debug.Log("SellList 생성");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        OnSellListChanged -= Calculate_data;
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
        foreach (int i in sellList.Keys)
        {
            total_gold += Items.Instance[i].price * sellList[i];
            total_weight += Items.Instance[i].weight * sellList[i];
        }

        Debug.Log("SellList's Total gold = " + total_gold);
        Debug.Log("SellList's Total weight = " + total_weight);

    }

    public void AddList(int item_id, int count)
    {

        if (sellList.ContainsKey(item_id))
        {
            if (Inventory.Instance.GetItemCount(item_id) < sellList[item_id] + count)
            {
                Debug.Log("보유한 아이템 보다 많이 팔 수 없습니다.");
                return;
            }
            sellList[item_id] += count;
            buttonList[item_id].add_count(count);

            Debug.Log("아이템 번호" + item_id + "이 " + count + "만큼 추가됨 총: " + sellList[item_id]);

        }
        else
        {
            if (Inventory.Instance.GetItemCount(item_id) < count)
            {
                Debug.Log("보유한 아이템 보다 많이 팔 수 없습니다.");
                return;
            }
            sellList.Add(item_id, count);
            GameObject itemObj = Instantiate(ItemButtonPrefeb, constentParent);
            Item_Button item_ButtonScript = itemObj.GetComponent<Item_Button>();
            item_ButtonScript.Set_SellListButton(Items.Instance[item_id], tradeUI, count);
            button_objectList.Add(item_id, itemObj);
            buttonList.Add(item_id, item_ButtonScript);
            Debug.Log("아이템 번호" + item_id + "가 " + count + "만큼 새로 추가됨 총: " + sellList[item_id]);

        }
        OnSellListChanged?.Invoke();
    }

    public void RemoveList(int item_id, int count)
    {
        if (sellList.ContainsKey(item_id))
        {
            sellList[item_id] -= count;
            buttonList[item_id].remove_count(count);

            Debug.Log("아이템 번호" + item_id + "이" + count + "만큼 감소됨");
            if (sellList[item_id] <= 0)
            {
                Destroy(buttonList[item_id]);
                Destroy(button_objectList[item_id]);
                buttonList.Remove(item_id);
                button_objectList.Remove(item_id);
                sellList.Remove(item_id);
                Debug.Log("아이템 리스트 삭제");
            }
            OnSellListChanged?.Invoke();
        }
        else
        {
            Debug.Log("잘못된 데이터 접근");
        }
    }

    public void Sell_items()
    {
        foreach (int item_id in sellList.Keys)
        {
            Inventory.Instance.RemoveItem(item_id, sellList[item_id]);

        }
        ResetList();

    }
    public void ResetList()
    {
        foreach (int item_id in buttonList.Keys)
        {
            Destroy(buttonList[item_id]);
            Destroy(button_objectList[item_id]);
        }

        buttonList.Clear();
        button_objectList.Clear();
        sellList.Clear();

        OnSellListChanged?.Invoke();
    }
}
