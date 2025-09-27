using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform spawnPoint; // 발사체가 생성될 위치
    public List<WeaponData> equippedWeapons; // 인스펙터에서 유저가 집어넣은 무기 정보

    //Dictionary 함수 각 아이템이 고유한 Key와 그에 해당하는 Value로 구성됨
    private Dictionary<WeaponData, float> weaponCooldowns; 
    void Start()
    {
        //Debug.Log("Equipped weapons count: " + equippedWeapons.Count);
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
               // Debug.Log("Attack condition met for: " + weapon.weaponName);
                Attack(weapon);
                //쿨타임 읽어옴
                weaponCooldowns[weapon] = Time.time + weapon.cooldown;
            }
        }
    }

    void Attack(WeaponData weapon)
    {
        // 1. 진짜 목표 지점을 찾는다.
        Vector3 targetPoint = FindTargetPoint();

        for (int i = 0; i < weapon.projectileAmount; i++)
        {
            // 2. 총구에서 목표 지점을 바라보는 방향을 계산한다.
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
        // 카메라 화면의 정중앙 좌표를 가져온다. (x: 0.5, y: 0.5)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;

        // Raycast를 쏴서 무언가에 부딪혔는지 확인한다. (사정거리는 1000 정도로 넉넉하게)
        // 플레이어 자신이나 투사체는 맞으면 안되므로, LayerMask를 사용하는 것이 좋다.
        // 여기서는 간단하게 플레이어 자신만 피하도록 처리.
        int layerMask = ~(1 << LayerMask.NameToLayer("Player")); // "Player" 레이어를 제외한 모든 레이어

        if (Physics.Raycast(ray, out hit, 1000f, layerMask))
        {
            // Ray가 부딪힌 지점을 목표 지점으로 설정한다.
            targetPoint = hit.point;
            Debug.DrawLine(ray.origin, hit.point, Color.green, 1f); // 디버깅용: 녹색 선
        }
        else
        {
            // Ray가 아무것에도 부딪히지 않았다면 (허공을 쏠 때),
            // 카메라 방향으로 아주 먼 지점을 목표로 설정한다.
            targetPoint = ray.GetPoint(1000f);
            Debug.DrawLine(ray.origin, targetPoint, Color.yellow, 1f); // 디버깅용: 노란 선
        }

        return targetPoint;
    }
}