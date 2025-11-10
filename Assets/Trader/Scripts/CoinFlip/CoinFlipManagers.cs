using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinFlipManager : MonoBehaviour
{
    public static CoinFlipManager Instance { get; private set; }

    //[SerializeField]
    //private CoinFlipUI coinFlipUI;

    private Action onHeadsCallback;
    private Action onTailsCallback;

    public GameObject flipUI;
    public CoinController coinController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void StartCoinFlip(Action onHeads, Action onTails)
    {
        this.onHeadsCallback = onHeads;
        this.onTailsCallback = onTails;

        StartCoroutine(CoinFlipSequence());
    }

    private IEnumerator CoinFlipSequence()
    {

        flipUI.SetActive(true);
        coinController.FlipCoin();
        yield return new WaitForSeconds(2.0f);
        bool isHeads = (UnityEngine.Random.Range(0, 2) == 0);

        if (isHeads)
        {

            onHeadsCallback?.Invoke();
        }
        else
        {

            onTailsCallback?.Invoke();
        }

       
        yield return new WaitForSeconds(1.5f);
        flipUI.SetActive(false);
    }

    public void RegisterUI(GameObject uiObject)
    {
        flipUI = uiObject;
    }


    public void RegisterCoinController(CoinController uiObject)
    {
        coinController = uiObject;
    }

}