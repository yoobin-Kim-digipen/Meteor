using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradeUI : MonoBehaviour
{
    //Trade 패널의 콘텐츠 모음
    [SerializeField] private GameObject[] content;

    //아이템 이미지
    public Image Image;
    public TextMeshProUGUI Item_Name;

    //Text 
    public TextMeshProUGUI purchase_count;
    public TextMeshProUGUI weight_num;
    public TextMeshProUGUI price_num;

    //버튼모음
    public Button plus_button;
    public Button minus_button;
    public Button bargin_button;
    public Button trade_button;

    //플레이어 골드
    public Gold player_gold;


    public ItemData itemdata;
    private ItemData previtemdata;

    //Trade 패널 변수
    private int items_counted = 1; // 몇개 살건지
    private int items_weight = 0;   // 아이템의 무게 총합
    private int items_price = 0;    // 아이템의 가격 총합



    void Start()
    {
        plus_button.onClick.AddListener(Count_plus);
        minus_button.onClick.AddListener(Count_minus);
        trade_button.onClick.AddListener(Trade);
        bargin_button.onClick.AddListener(bargin);
        //content.SetActive(false);
        foreach (var c in content)
        {
            c.SetActive(false);
        }
        Debug.Log(" 트레이드 UI 생성됨");
    }

    void Update()
    {

        if (itemdata != previtemdata)
        {
            previtemdata = itemdata; // 현재 상태 저장

            if (itemdata == null)
            {
                Debug.Log("선택된 아이템 없음");
                foreach (var c in content)
                {
                    c.SetActive(false);
                }
            }
            else
            {
                Debug.Log("선택된 아이템 있음");
                trade_reset();
                foreach (var c in content)
                {
                    c.SetActive(true);
                }
                Image.sprite = Resources.Load<Sprite>(itemdata.image_path);
                Item_Name.text = itemdata.name;
            }
        }

        if (itemdata != null)
        {
            items_weight = itemdata.weight * items_counted;
            items_price = itemdata.price * items_counted;

            purchase_count.text = items_counted.ToString();
            weight_num.text = items_weight.ToString();
            price_num.text = items_price.ToString() + "G";

        }
    }

    public void Set_itemdata(ItemData item)
    {

        Debug.Log("아이템 선택됨");
        itemdata = item;
    }

    public void Count_plus()
    {
        items_counted++;
    }

    public void Count_minus()
    {
        if (items_counted <= 1)
            return;
        items_counted--;
    }

    public void Bargin()
    {
        //동전던지기
    }

    public void Trade()
    {
        if (player_gold.Use_gold(items_price))
        {
            Debug.Log("거래성공~");
        }
        else
        {
            Debug.Log("거래실패~");
        }

    }

    public void bargin()
    {
        if(items_price > 0)
        {
            CoinFlipManager.Instance.StartCoinFlip(
                () => {
                    Debug.Log("코인이 앞면으로 나왔습니다!");
                    items_price = (int)((float)items_price * 0.7f);
                    CoinFlipManager.Instance.flipUI.GetComponent<CoinFlipUI>().textUI.text = "SUCCES " + items_price.ToString();
                },
                () => {
                    Debug.Log("코인이 뒷면으로 나왔습니다!");
                    CoinFlipManager.Instance.flipUI.GetComponent<CoinFlipUI>().textUI.text = "FAILED " + items_price.ToString();
                }
            );
        }
        

    }


    public void trade_reset()
    {
        items_counted = 1;
        items_price = 0;
        items_weight = 0;
    }
}
