using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; 
    public float currentHealth; 

    void OnEnable()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
