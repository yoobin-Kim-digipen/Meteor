using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ColleagueButton : MonoBehaviour
{
    public Button mybutton;
    public Image Image;
    private ColleagueData colleagueData;


    public TextMeshProUGUI Name;
    public TextMeshProUGUI Class;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mybutton.onClick.AddListener(OnButtonClicked);
        Image.sprite = Resources.Load<Sprite>(colleagueData.image_path);
        Name.text = colleagueData.name;
        Class.text = colleagueData.Class;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClicked()
    {
        PubManager.Instance.Sellect(colleagueData);
        Debug.Log("¹öÆ° Å¬¸¯µÊ!");
    }

    public void Set(ColleagueData colleague)
    {
        colleagueData = colleague;
    }

}
