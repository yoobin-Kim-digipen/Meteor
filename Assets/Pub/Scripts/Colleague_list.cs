using UnityEngine;

public class Colleague_list : MonoBehaviour
{
    public Transform constentParent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PubManager.Instance.AddColleagueButton(constentParent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
