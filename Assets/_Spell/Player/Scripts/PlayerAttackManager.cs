using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform spawnPoint; // �߻�ü�� ������ ��ġ
    public List<WeaponData> equippedWeapons; // �ν����Ϳ��� ������ ������� ���� ����

    //Dictionary �Լ� �� �������� ������ Key�� �׿� �ش��ϴ� Value�� ������
    private Dictionary<WeaponData, float> weaponCooldowns; 
    void Start()
    {
        //Debug.Log("Equipped weapons count: " + equippedWeapons.Count);
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
               // Debug.Log("Attack condition met for: " + weapon.weaponName);
                Attack(weapon);
                //��Ÿ�� �о��
                weaponCooldowns[weapon] = Time.time + weapon.cooldown;
            }
        }
    }

    void Attack(WeaponData weapon)
    {
        // 1. ��¥ ��ǥ ������ ã�´�.
        Vector3 targetPoint = FindTargetPoint();

        for (int i = 0; i < weapon.projectileAmount; i++)
        {
            // 2. �ѱ����� ��ǥ ������ �ٶ󺸴� ������ ����Ѵ�.
            Vector3 directionToTarget = (targetPoint - spawnPoint.position).normalized;
            Quaternion spawnRotation = Quaternion.LookRotation(directionToTarget);

            string poolTag = weapon.weaponName;
            GameObject projectileObj = ObjectPooler.Instance.GetFromPool(poolTag, spawnPoint.position, spawnRotation);

            if (projectileObj == null) continue;

            Projectile projectileScript = projectileObj.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(weapon);
            }
        }
    }

    private Vector3 FindTargetPoint()
    {
        // ī�޶� ȭ���� ���߾� ��ǥ�� �����´�. (x: 0.5, y: 0.5)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;

        // Raycast�� ���� ���𰡿� �ε������� Ȯ���Ѵ�. (�����Ÿ��� 1000 ������ �˳��ϰ�)
        // �÷��̾� �ڽ��̳� ����ü�� ������ �ȵǹǷ�, LayerMask�� ����ϴ� ���� ����.
        // ���⼭�� �����ϰ� �÷��̾� �ڽŸ� ���ϵ��� ó��.
        int layerMask = ~(1 << LayerMask.NameToLayer("Player")); // "Player" ���̾ ������ ��� ���̾�

        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            // Ray�� �ε��� ������ ��ǥ �������� �����Ѵ�.
            targetPoint = hit.point;
            Debug.DrawLine(ray.origin, hit.point, Color.green, 1f); // ������: ��� ��
        }
        else
        {
            // Ray�� �ƹ��Ϳ��� �ε����� �ʾҴٸ� (����� �� ��),
            // ī�޶� �������� ���� �� ������ ��ǥ�� �����Ѵ�.
            targetPoint = ray.GetPoint(1000f);
            Debug.DrawLine(ray.origin, targetPoint, Color.yellow, 1f); // ������: ��� ��
        }

        return targetPoint;
    }
}