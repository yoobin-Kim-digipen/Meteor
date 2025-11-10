using NUnit.Framework.Interfaces;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PubManager : MonoBehaviour
{
    public static PubManager Instance { get; private set; }
    //동료 리스트
    private Colleagues colleagues = new Colleagues();
    //버튼 프리팹
    public GameObject colleagueButtonPrefeb;

    //선택된 동료 전시
    public Image selected_image;
    public TextMeshProUGUI selected_name;
    public TextMeshProUGUI selected_class;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); 
        }
    }

    private void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddColleagueButton(Transform constentParent)
    {
        Debug.Log("동료버튼추가");
        foreach (ColleagueData colleague in colleagues.colleagueList)
        {

            GameObject colleagueObj = Instantiate(colleagueButtonPrefeb, constentParent);
            ColleagueButton colleagueButtonScript = colleagueObj.GetComponent<ColleagueButton>();
            colleagueButtonScript.Set(colleague);

        }
    }

    public void Sellect(ColleagueData colleague)
    {
        selected_image.sprite = Resources.Load<Sprite>(colleague.image_path);
        selected_name.text = colleague.name; 
        selected_class.text = colleague.Class;
    }
}
