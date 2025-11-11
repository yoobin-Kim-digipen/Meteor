using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using static UnityEditor.Progress;
public class Inventory : MonoBehaviour
{

    public static Inventory Instance { get; private set; }
    //인벤토리의 아이템들을 <아이템 번호, 갯수> 로 관리
    private Dictionary<int, int> itemDictionary = new Dictionary<int, int>();

    //마굿간의 최대무게
    private int limit_weight = 300;
    private int weight = 0;

    private int preCount = 0;
    private void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (preCount != itemDictionary.Values.Sum())
        {
            weight = 0;
            foreach (int i in itemDictionary.Keys)
            {
                weight += Items.Instance.itemList[i - 1].weight * itemDictionary[i];
            }

            Debug.Log("Inventory weight = " + weight);

            preCount = itemDictionary.Values.Sum();
        }
    }

    public bool AddItem(int item_id, int count)
    {
        int items_weight = Items.Instance.itemList[item_id - 1].weight * count;

        if (limit_weight >= items_weight)
        {
            if (itemDictionary.ContainsKey(item_id))
            {

                itemDictionary[item_id] += count;
                Debug.Log("아이템 번호" + item_id + "이 " + count + "만큼 인벤토리에 추가됨 총: " + itemDictionary[item_id]);

            }
            else
            {
                itemDictionary.Add(item_id, count);


                Debug.Log("아이템 번호" + item_id + "가 " + count + "만큼 새로 인벤토리에 추가됨 총: " + itemDictionary[item_id]);

            }
            return true;
        }
        else
        {
            Debug.Log("용량이 부족합니다.");
            return false;
        }

    }
    public void RemoveItem(int item_id, int count)
    {
        if (itemDictionary.ContainsKey(item_id))
        {

            itemDictionary[item_id] -= count;
            Debug.Log("아이템 번호" + item_id + "이 " + count + "만큼 제거됨 총: " + itemDictionary[item_id]);

        }
        else
        {
            Debug.Log("잘못된 데이터 접근");
        }
    }

    //인벤토리 초기화
    public void ClearInventory()
    {
        itemDictionary.Clear();
    }

    //아이템 개수 가져오기
    public int GetItemCount(int itemID)
    {
        return itemDictionary.TryGetValue(itemID, out int count) ? count : 0;
    }

    public int GetLimitWeight()
    {
        return limit_weight;
    }

    public int GetAvailableWeight()
    {
        return limit_weight - weight;
    }


}
