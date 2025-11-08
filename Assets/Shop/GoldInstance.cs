using UnityEngine;

public class Gold : MonoBehaviour
{
    [SerializeField] private int gold = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Gold °´Ã¼ »ý¼ºµÊ");
        gold = 100;
    }

    public bool Use_gold(int amount)
    {
        if (gold < amount)
        {
            Debug.Log("°ñµå°¡ ºÎÁ·ÇÕ´Ï´Ù.");
            return false;
        }
        else
        {
            this.gold -= amount;
        }
        return true;
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
