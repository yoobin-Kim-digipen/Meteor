using UnityEngine;

[CreateAssetMenu(fileName = "New WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName; // �� �̸��� Ǯ(Pool)�� ���� �ĺ���(Tag)�� ����.
    public string description;

    [Header("Stats")]
    public float damage;
    public float cooldown;
    public int projectileAmount;

    [Header("Pooling")]
    public int poolSize = 20; // �� ���⸦ ���� �̸� ������ �� ����ü�� ����

    [Header("Projectile")]
    public GameObject projectilePrefab; // �߻��� ����ü�� ������
    public float projectileSpeed;
    public float projectileLifetime;
}
