using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuyList : MonoBehaviour
{
    public TradeUI tradeUI;

    public GameObject ItemButtonPrefeb;
    //리스트가 생성될 곳
    public Transform constentParent;

    //
    public Items items;

    //구매목록 버튼
    private Dictionary<int, GameObject> button_objectList = new Dictionary<int, GameObject>();
    private Dictionary<int, Item_Button> buttonList = new Dictionary<int,Item_Button>();
    //구매목록
    private Dictionary<int,int> buyList = new Dictionary<int, int>();

    private int preCount = 0;

    private int total_gold = 0;
    private int total_weight = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("BuyList 생성");
    }

    // Update is called once per frame
    void Update()
    {
        //buyList의 밸류값의 총합을 구해야함.
        if(preCount != buyList.Values.Sum())
        {
            total_gold = 0;
            total_weight = 0;
            foreach(int i in buyList.Keys)
            {
                total_gold += items.itemList[i - 1].price * buyList[i];
                total_weight += items.itemList[i - 1].weight * buyList[i];
            }

            Debug.Log("Total gold = " + total_gold);
            Debug.Log("Total weight = " + total_weight);

            preCount = buyList.Values.Sum();
        }
    }

    public int Get_totalgold()
    {
        return total_gold;
    }
    public int Get_totalweight()
    {
        return total_weight;
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
            item_ButtonScript.Set_BuyListButton(items.itemList[item_id - 1], tradeUI, count);
            button_objectList.Add(item_id, itemObj);
            buttonList.Add(item_id, item_ButtonScript);
            Debug.Log("아이템 번호" + item_id + "가 " + count + "만큼 새로 추가됨 총: " + buyList[item_id]);

        }
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
        }
        else
        {
            Debug.Log("잘못된 데이터 접근");
        }
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
    }

}
