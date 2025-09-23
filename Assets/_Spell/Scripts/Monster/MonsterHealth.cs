using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f; // 최대 체력을 변수로 관리
    private float health; // 현재 체력은 내부에서만 관리

    // 오브젝트가 풀에서 나와 활성화될 때마다 호출됨
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
        }
    }
}