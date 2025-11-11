using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Display_Gold : MonoBehaviour
{
    public Gold gold;
    public TextMeshProUGUI inventory_weight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<TMP_Text>().text = "Player Gold : " + gold.GoldAmount.ToString();
        inventory_weight.text = "Limit Weight : " + Inventory.Instance.GetLimitWeight() + " Current Weight : " + Inventory.Instance.GetCurrentWeight();
    }
}
