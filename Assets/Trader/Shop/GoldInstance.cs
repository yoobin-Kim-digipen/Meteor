using UnityEngine;

public class Gold : MonoBehaviour
{
    [SerializeField] private int gold = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Gold 객체 생성됨");
        gold = 100;
    }

    public bool Use_gold(int amount)
    {
        if (gold < amount)
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }
        else
        {
            this.gold -= amount;
        }
        return true;
    }

    public void Add_Gold(int amount)
    {
        this.gold += amount;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public int GoldAmount
    {
        get { return gold; }
    }
}
