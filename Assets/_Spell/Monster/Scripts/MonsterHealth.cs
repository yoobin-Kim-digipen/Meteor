using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f; // �ִ� ü���� ������ ����
    private float health; // ���� ü���� ���ο����� ����

    // ������Ʈ�� Ǯ���� ���� Ȱ��ȭ�� ������ ȣ���
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