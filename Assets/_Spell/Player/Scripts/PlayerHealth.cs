using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 200f; 
    public float currentHealth; 

    void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        print($"플레이어가 {amount} 만큼 공격받았습니다!, 현재 체력: {currentHealth}");
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
