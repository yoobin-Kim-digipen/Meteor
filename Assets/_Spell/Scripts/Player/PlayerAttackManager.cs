using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform spawnPoint; // �߻�ü�� ������ ��ġ
    public List<WeaponData> equippedWeapons; // �ν����Ϳ��� ������ ������� ���� ����

    //Dictionary �Լ� �� �������� ������ Key�� �׿� �ش��ϴ� Value�� ������
    Dictionary<WeaponData, float> weaponCooldowns; 
    void Start()
    {
        Debug.Log("Equipped weapons count: " + equippedWeapons.Count);
        weaponCooldowns = new Dictionary<WeaponData, float>();

        //foreach (var ������ in �÷���) �̷� ��ɵ� �ֳ� ���� �ű��ϳ�
            foreach (var weapon in equippedWeapons)
        {
            // ��ٿ� Ÿ�̸� �ʱ�ȭ(� ��ų�̵� ��ź ��������)
            weaponCooldowns[weapon] = 0f;
        }
    }

    void Update()
    {
        foreach (var weapon in equippedWeapons)
        {
            //ó���� 0�ʴϱ� �ϴ� ��� �� �����ִ� ����
            if (Time.time >= weaponCooldowns[weapon])
            {
                Debug.Log("Attack condition met for: " + weapon.weaponName);
                Attack(weapon);
                //��Ÿ�� �о��
                weaponCooldowns[weapon] = Time.time + weapon.cooldown;
            }
        }
    }

    void Attack(WeaponData weapon)
    {
        for (int i = 0; i < weapon.projectileAmount; i++)
        {
            Quaternion spawnRotation = FindBestTargetDirection();
            string poolTag = weapon.weaponName;

            GameObject projectileObj = ObjectPooler.Instance.GetFromPool(poolTag, spawnPoint.position, spawnRotation);

            if (projectileObj == null) continue;

            Projectile projectileScript = projectileObj.GetComponent<Projectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(weapon);

                //this.speed = weaponData.projectileSpeed;   <- �̷��͵� ����
                //this.lifetime = weaponData.projectileLifetime;
                //this.damage = weaponData.damage;
            }
        }
    }

    private Quaternion FindBestTargetDirection()
    {
        // TODO: �ֺ��� ���� ã�� ������ ���ϴ� ����
        // ������ �ϴ� �÷��̾��� �������� �߻�
        return Camera.main.transform.rotation; // <- ���� ī�޶��� ȸ����
    }
}