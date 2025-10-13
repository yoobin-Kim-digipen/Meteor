using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Attack Settings")]
    public List<WeaponData> equippedWeapons; // 인스펙터에서 유저가 집어넣은 무기 정보
    private Dictionary<WeaponData, float> weaponCooldowns;

    [Header("Aiming Settings")]
    public float rotationSpeed = 720f;

    private Rigidbody _playerRb;
    private Camera _mainCam;
    private int _layerMask;
    // NonAlloc을 위한 결과 저장용 배열. 우리는 하나만 필요하므로 크기는 1.
    private readonly RaycastHit[] _raycastHits = new RaycastHit[1];

    void Start()
    {
        _playerRb = GetComponentInParent<Rigidbody>();
        _mainCam = Camera.main;
        _layerMask = ~(1 << LayerMask.NameToLayer("Player"));

        weaponCooldowns = new Dictionary<WeaponData, float>();

        foreach (var weapon in equippedWeapons)
        {
            weaponCooldowns[weapon] = 0f;
        }
    }

    public void HandleAimAndAttack()
    {
        if (equippedWeapons.Count == 0 || _playerRb == null) return;

        Vector3 targetPoint = FindTargetPoint();
        RotateBodyTowards(targetPoint);

        WeaponData currentWeapon = equippedWeapons[0];
        TryAttack(currentWeapon, targetPoint);
    }

    private void RotateBodyTowards(Vector3 targetPoint)
    {
        Vector3 direction = (targetPoint - _playerRb.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion newRotation = Quaternion.RotateTowards(_playerRb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        _playerRb.MoveRotation(newRotation);
    }

    private void TryAttack(WeaponData weapon, Vector3 targetPoint)
    {
        if (Time.time >= weaponCooldowns[weapon])
        {
            Attack(weapon, targetPoint);
            weaponCooldowns[weapon] = Time.time + weapon.cooldown;
        }
    }

    void Attack(WeaponData weapon, Vector3 targetPoint)
    {
        // 1. 사용할 스킬이 있는지 확인
        if (weapon.skills == null || weapon.skills.Count == 0) return;

        // (지금은 첫 번째 스킬만 사용)
        SkillData skillToUse = weapon.skills[0];

        // 스폰 위치/회전 계산
        if (skillToUse.spawnStrategy == null)
        {
            Debug.LogError(skillToUse.name + "에 SpawnStrategy가 할당되지 않았습니다!");
            return;
        }

        skillToUse.spawnStrategy.CalculateSpawnTransform(transform, targetPoint, skillToUse, out Vector3 spawnPos, out Quaternion baseRotation);


        // 발사 패턴 실행
        IFirePattern firePattern = skillToUse.GetFirePattern();
        if (firePattern == null)
        {
            Debug.LogError(skillToUse.name + "에 FirePattern이 정의되지 않았습니다!");
            return;
        }

        // 발사 패턴에게 "계산된 위치와 방향으로 발사를 실행해!" 라고 모든 것을 위임
        firePattern.Execute(gameObject, skillToUse, spawnPos, baseRotation, targetPoint);
    }

    private Vector3 FindTargetPoint()
    {
        // 카메라 화면의 정중앙 좌표를 가져온다. (x: 0.5, y: 0.5)
        Ray ray = _mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;
        int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, 1000f, _layerMask);

        if (hitCount > 0) // 한 개 이상 맞았다면
        {
            // Ray가 부딪힌 지점을 목표 지점으로 설정한다.
            targetPoint = _raycastHits[0].point;
            Debug.DrawLine(ray.origin, targetPoint, Color.green, 1f); // 디버깅용: 녹색 선
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