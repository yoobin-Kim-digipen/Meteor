using UnityEngine;
using System.Collections;
public class Projectile : MonoBehaviour
{
    private float speed;
    private float lifetime;
    private float damage;
    private Rigidbody rb;

    public void Initialize(WeaponData weaponData)
    {
        this.speed = weaponData.projectileSpeed;
        this.lifetime = weaponData.projectileLifetime;
        this.damage = weaponData.damage;
    }

    void Awake()
    {
        //Debug.Log(gameObject.name + " Awake! Trying to get Rigidbody.");
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(gameObject.name + " could NOT find Rigidbody component!");
        }
    }

    // OnEnable�Լ� <- start�� awake�� �޸� ������Ʈ�� ��Ȱ��ȭ �Ǿ��ٰ� �ٽ� Ȱ��ȭ �ɶ��� ȣ���
    void OnEnable()
    {
        rb.linearVelocity = transform.forward * speed;

        // �Ű�������� �ڷ�ƾ(�ð����� �ΰ� ����Ǵ� �Լ�)�� ���� / �߻�ü ��Ȱ��ȭ Ÿ�̸�
        // �׳� �װ��� ���ư��ٰ� �� �ƹ� �浹���µ� ���ʵڿ� ������ �׷���
        StartCoroutine(DeactivateAfterTime(lifetime));
    }

    IEnumerator DeactivateAfterTime(float time) // ��ȯ Ÿ��: �ڷ�ƾ �Լ��� return Ÿ���� �ݵ�� IEnumerator ���� ��.
    {
        // �Լ����� �Ͻ����� return���� ���������°��� �ƴ�
        yield return new WaitForSeconds(time);

        // yield return �ð��� ���� �� setactive ����(������Ʈ ��Ȱ��ȭ)�� ����
        gameObject.SetActive(false);
    }

    // Ʈ�����浹(collider�� �������� �˷���)�� �߻������� ����Ƽ ������ ȣ��
    void OnTriggerEnter(Collider other)
    {
        // EnemyHealth ��ũ��Ʈ�� ���� ���� �ε������� Ȯ��
        MonsterHealth enemy = other.GetComponent<MonsterHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}