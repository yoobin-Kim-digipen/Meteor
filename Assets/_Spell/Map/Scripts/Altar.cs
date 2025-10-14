using UnityEngine;

public class Altar : MonoBehaviour
{
    private bool playerInRange = false;
    private int xpAmount;

    void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("제단에 봉헌 중...");
                xpAmount = StatManager.Instance.GetXP();
                if (StatManager.Instance.GetRequiredXP() <= xpAmount)
                {
                    StatManager.Instance.LevelUp();
                    Debug.Log("제단에 봉헌 완료.");
                }
                else
                {
                    Debug.Log("경험치가 부족합니다.");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("제단에 봉헌 가능. G키를 눌러 봉헌하세요.");
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}

