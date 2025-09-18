using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform spawnPoint; // 발사체가 생성될 위치
    public List<WeaponData> equippedWeapons; // 인스펙터에서 유저가 집어넣은 무기 정보

    //Dictionary 함수 각 아이템이 고유한 Key와 그에 해당하는 Value로 구성됨
    Dictionary<WeaponData, float> weaponCooldowns; 
    void Start()
    {
        Debug.Log("Equipped weapons count: " + equippedWeapons.Count);
        weaponCooldowns = new Dictionary<WeaponData, float>();

        //foreach (var 아이템 in 컬렉션) 이런 기능도 있노 종나 신기하네
            foreach (var weapon in equippedWeapons)
        {
            // 쿨다운 타이머 초기화(어떤 스킬이든 초탄 장전상태)
            weaponCooldowns[weapon] = 0f;
        }
    }

    void Update()
    {
        foreach (var weapon in equippedWeapons)
        {
            //처음엔 0초니까 일단 쏘고 쿨 정해주는 느낌
            if (Time.time >= weaponCooldowns[weapon])
            {
                Debug.Log("Attack condition met for: " + weapon.weaponName);
                Attack(weapon);
                //쿨타임 읽어옴
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

                //this.speed = weaponData.projectileSpeed;   <- 이런것들 해줌
                //this.lifetime = weaponData.projectileLifetime;
                //this.damage = weaponData.damage;
            }
        }
    }

    private Quaternion FindBestTargetDirection()
    {
        // TODO: 주변의 적을 찾아 방향을 정하는 로직
        // 지금은 일단 플레이어의 정면으로 발사
        return Camera.main.transform.rotation; // <- 메인 카메라의 회전값
    }
}