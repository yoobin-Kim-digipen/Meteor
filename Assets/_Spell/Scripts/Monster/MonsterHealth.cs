using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float health = 50f;
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage. Health: " + health);
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}