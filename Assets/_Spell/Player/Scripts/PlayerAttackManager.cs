using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Attack Settings")]
    public List<WeaponData> equippedWeapons; // 인스펙터에서 유저가 집어넣은 무기 정보
    private Dictionary<WeaponData, float> weaponCooldowns;
    private bool _isFirstShot = true;

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

    public void OnStopAttack()
    {
        _isFirstShot = true;
    }

    private void TryAttack(WeaponData weapon, Vector3 targetPoint)
    {
        if (Time.time >= weaponCooldowns[weapon])
        {
            Attack(weapon, targetPoint);
            weaponCooldowns[weapon] = Time.time + weapon.cooldown;
            _isFirstShot = false;
        }
    }

    void Attack(WeaponData weapon, Vector3 targetPoint)
    {
        if (weapon.skills == null || weapon.skills.Count == 0) return;

        SkillData skillToUse = weapon.skills[0];

        Vector3 spawnPos;
        Quaternion spawnRotation;

        // SkillData에 정의된 스폰 타입에 따라 위치와 회전값을 계산
        switch (skillToUse.spawnType)
        {
            case SkillSpawnType.FromCaster:
                // 기본 스폰 위치는 이제 플레이어의 중심 위치
                Vector3 baseSpawnPos = transform.position;

                // 첫 발 보정 트릭: 이상적인 회전값을 기준으로 오프셋을 적용합니다.
                if (_isFirstShot)
                {
                    Vector3 idealDirection = (targetPoint - baseSpawnPos).normalized;
                    idealDirection.y = 0f;
                    Quaternion idealRotation = Quaternion.LookRotation(idealDirection);
                    // 이상적인 회전 * 로컬 오프셋 = 월드 오프셋
                    spawnPos = baseSpawnPos + idealRotation * skillToUse.spawnOffset;
                }
                else
                {
                    // 두 번째 발부터는 현재 캐릭터의 회전을 기준으로 오프셋을 적용
                    spawnPos = baseSpawnPos + transform.rotation * skillToUse.spawnOffset;
                }

                spawnRotation = Quaternion.LookRotation((targetPoint - spawnPos).normalized);
                break;

            case SkillSpawnType.OnTarget:
                spawnPos = targetPoint + Vector3.up * skillToUse.spawnHeightOffset;
                spawnRotation = Quaternion.LookRotation(Vector3.down);
                break;

            default: // 예외 처리
                spawnPos = transform.position;
                spawnRotation = Quaternion.identity;
                break;
        }

        GameObject skillObj = ObjectPooler.Instance.GetFromPool(skillToUse.skillName, spawnPos, spawnRotation);

        if (skillObj != null)
        {
            Skill skill = skillObj.GetComponent<Skill>();
            if (skill != null)
            {
                skill.Activate(gameObject, skillToUse);
            }
        }
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