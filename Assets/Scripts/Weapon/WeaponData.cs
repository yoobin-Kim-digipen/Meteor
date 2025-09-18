using UnityEngine;

[CreateAssetMenu(fileName = "New WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName; // 이 이름이 풀(Pool)의 고유 식별자(Tag)로 사용됨.
    public string description;

    [Header("Stats")]
    public float damage;
    public float cooldown;
    public int projectileAmount;

    [Header("Pooling")]
    public int poolSize = 20; // 이 무기를 위해 미리 생성해 둘 투사체의 개수

    [Header("Projectile")]
    public GameObject projectilePrefab; // 발사할 투사체의 프리팹
    public float projectileSpeed;
    public float projectileLifetime;
}
