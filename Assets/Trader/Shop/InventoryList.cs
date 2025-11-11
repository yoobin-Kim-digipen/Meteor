using System.Collections.Generic;
using UnityEngine;

public class InventoryList : MonoBehaviour
{
    [Header("UI References")]
    public TradeUI tradeUI;
    public GameObject ItemButtonPrefeb;
    public Transform constentParent;

    // 오브젝트 풀
    //private List<Item_Button> buttonList = new List<Item_Button>();
    private Dictionary<int, GameObject> button_objectList = new Dictionary<int, GameObject>();

    void Start()
    {
        Debug.Log("InventoryList 생성");

        // 이벤트 구독
        Inventory.Instance.OnInventoryChanged += RefreshInventoryUI;
        Inventory.Instance.Item_delete += Remove_Button;
        // 초기UI
        RefreshInventoryUI();

    }

    private void OnDestroy()
    {
        //이벤트 구독 해제 (메모리 누수 방지)
        if (Inventory.Instance != null)
        {
            Inventory.Instance.OnInventoryChanged -= RefreshInventoryUI;
            Inventory.Instance.Item_delete -= Remove_Button;

        }

    }

    void Update()
    {

    }

    public void Remove_Button(int item_id)
    {
        if(button_objectList.ContainsKey(item_id))
        {
            Destroy(button_objectList[item_id]);
            button_objectList.Remove(item_id);
        }
    }

    private void RefreshInventoryUI()
    {
        var itemDict = Inventory.Instance.GetItemDictionary();
        
        foreach(int item_id in itemDict.Keys)
        {
            if (button_objectList.ContainsKey(item_id))
            {
                button_objectList[item_id].GetComponent<Item_Button>().SetCount(itemDict[item_id]);
            }
            else
            {
                GameObject obj = Instantiate(ItemButtonPrefeb, constentParent);
                Item_Button itemButton = obj.GetComponent<Item_Button>();
                itemButton.Set_InventroyButton(Items.Instance[item_id], tradeUI, Inventory.Instance.GetItemCount(item_id));
                button_objectList.Add(item_id, obj);
            }
        }
        //int i = 0; // 풀링 인덱스

        //foreach (int item_id in itemDict.Keys)
        //{
        //    if (i >= buttonList.Count)
        //    {
        //        GameObject obj = Instantiate(ItemButtonPrefeb, constentParent);
        //        Item_Button newButton = obj.GetComponent<Item_Button>();
        //        buttonList.Add(newButton);
        //    }

        //    var button = buttonList[i];
        //    button.gameObject.SetActive(true);
        //    button.Set_InventroyButton(
        //        Items.Instance[item_id],
        //        tradeUI,
        //        Inventory.Instance.GetItemCount(item_id)
        //    );

        //    i++;
        //}

        //for (; i < buttonList.Count; i++)
        //{
        //    buttonList[i].gameObject.SetActive(false);
        //}

        Debug.Log("InventoryList 갱신 완료 (" + itemDict.Count + " items)");
    }
}