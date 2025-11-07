using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackManager : MonoBehaviour
{
    [Header("Attack Settings")]
    public List<WeaponData> equippedWeapons;
    private Dictionary<WeaponData, float> weaponCooldowns;
    private int _currentSkillIndex = 0;

    [Header("Aiming Settings")]
    public float rotationSpeed = 720f;

    private Rigidbody _playerRb;
    private Camera _mainCam;
    private int _layerMask;
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

    void Update()
    {
        HandleSkillSwitching();
    }

    private void HandleSkillSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchSkill(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchSkill(1);
    }

    private void SwitchSkill(int index)
    {
        if (equippedWeapons.Count == 0) return;
        WeaponData currentWeapon = equippedWeapons[0];

        if (index >= 0 && index < currentWeapon.skills.Count)
        {
            _currentSkillIndex = index;
            Debug.Log($"스킬 '{currentWeapon.skills[_currentSkillIndex].skillName}'(으)로 교체했습니다.");
        }
        else
        {
            Debug.LogWarning($"현재 무기에 {index + 1}번 스킬 슬롯이 없습니다.");
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

    // 일반 공격 (마우스 클릭) 시 호출
    void Attack(WeaponData weapon, Vector3 targetPoint)
    {
        if (weapon.skills == null || weapon.skills.Count == 0 || _currentSkillIndex >= weapon.skills.Count) return;
        SkillData skillToUse = weapon.skills[_currentSkillIndex];

        // 일반 공격임을 표시하며 발사 로직 실행
        FireSkillInternal(skillToUse, gameObject, isSynergy: false);
        StatManager.Instance.AdjustingMP(-5);
    }

    // 외부(StatManager)에서 연계 스킬 사용 시 호출
    public void UseSkill(SkillData skillData, GameObject caster)
    {
        StatManager.Instance.AdjustingMP(-5);
        // 연계 공격임을 표시하며 발사 로직 실행
        FireSkillInternal(skillData, caster, isSynergy: true);
    }

    // 실제 발사를 담당하는 통합 함수
    private void FireSkillInternal(SkillData skillData, GameObject caster, bool isSynergy)
    {
        if (skillData == null || caster == null) return;

        int projectileCount = 1;
        
        // 발사하려는 스킬이 유도탄 스킬인지 확인
        if (skillData is HomingProjectileSkillData homingSkill)
        {
            HomingProjectileSkillData tempSkillData = Instantiate(homingSkill);
            tempSkillData.damage += StatManager.Instance.bonusHomingDamage;
            skillData = tempSkillData; // 강화된 '일회용' 데이터로 교체

            // 일반 공격일 때만 StatManager로부터 추가 발사체 수를 가져옴
            if (!isSynergy)
            {
                projectileCount = 1 + StatManager.Instance.bonusNormalHomingProjectiles;
            }
        }

        Vector3 targetPoint = FindTargetPoint();
        if (skillData.spawnStrategy == null) { Debug.LogError(skillData.name + "에 SpawnStrategy가 없습니다!"); return; }
        skillData.spawnStrategy.CalculateSpawnTransform(transform, targetPoint, skillData, out Vector3 spawnPos, out Quaternion baseRotation);

        IFirePattern firePattern = skillData.GetFirePattern();
        if (firePattern == null) { Debug.LogError(skillData.name + "에 FirePattern이 없습니다!"); return; }

        for (int i = 0; i < projectileCount; i++)
        {
            Quaternion finalRotation = baseRotation;
            if (projectileCount > 1)
            {
                float spreadAngle = Random.Range(-5f, 5f);
                finalRotation *= Quaternion.Euler(0, spreadAngle, 0);
            }
            firePattern.Execute(caster, skillData, spawnPos, finalRotation, targetPoint);
        }
    }

    private Vector3 FindTargetPoint()
    {
        Ray ray = _mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;
        int hitCount = Physics.RaycastNonAlloc(ray, _raycastHits, 1000f, _layerMask);
        if (hitCount > 0) { targetPoint = _raycastHits[0].point; }
        else { targetPoint = ray.GetPoint(1000f); }
        return targetPoint;
    }
}

