using System;
using System.Collections;
using UnityEngine;

public class CoinFlipManager : MonoBehaviour
{
    public static CoinFlipManager Instance { get; private set; }

    //[SerializeField]
    //private CoinFlipUI coinFlipUI;

    private Action onHeadsCallback; 
    private Action onTailsCallback; 

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
    }
}

