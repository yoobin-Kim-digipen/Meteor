using UnityEngine;
using System.Collections.Generic;

public class MonsterHealth : MonoBehaviour
{
    public float maxHealth = 50f; 
    private float health; 

    void OnEnable()
    {
        health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        //Debug.Log(gameObject.name + " took " + amount + " damage. Health: " + health);
        if (health <= 0)
        {
            gameObject.SetActive(false);
            StatManager.Instance.GainExperience(10);
        }
    }
}