using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingProjectile : Skill // Skill을 상속받습니다.
{
    private HomingProjectileSkillData _data;
    private Rigidbody _rb;
    private GameObject _caster;
    private Transform _currentTarget; // 현재 추적할 대상
    private string _targetTag; // 공격 대상 태그 (플레이어 vs 적)
    private bool _isPlayerProjectile = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // 스킬 핸들러로부터 호출되는 초기화 함수
    public override void Activate(GameObject caster, SkillData data)
    {
        _caster = caster;
        if (data is HomingProjectileSkillData homingData)
        {
            _data = homingData;

            if (caster.CompareTag("Player"))
            {
                _targetTag = "Enemy";
                _isPlayerProjectile = true; // This is a player's projectile
            }
            else if (caster.CompareTag("Enemy"))
            {
                _targetTag = "Player";
                _isPlayerProjectile = false;
            }
            
            _currentTarget = null;
            StartCoroutine(DeactivateAfterTime(_data.lifetime));
        }
    }

    void FixedUpdate()
    {
        if (_data == null) return;

        float timeMultiplier = 1.0f;
        // If this is a player projectile, get the time scale multiplier
        if (_isPlayerProjectile && StatManager.Instance != null)
        {
            timeMultiplier = StatManager.Instance.PlayerTimeScaleMultiplier;
        }

        if (_currentTarget == null || !_currentTarget.gameObject.activeSelf)
        {
            FindNewTarget();
        }

        if (_currentTarget != null)
        {
            Vector3 directionToTarget = (_currentTarget.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // Also apply the multiplier to the rotation speed to maintain smooth turning
            float finalRotationSpeed = _data.rotationSpeed * timeMultiplier;
            _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, finalRotationSpeed * Time.fixedDeltaTime);
        }

        // Constantly update the velocity with the time multiplier
        _rb.linearVelocity = transform.forward * _data.speed * timeMultiplier;
    }

    void FindNewTarget()
    {
        // 주변에서 _targetTag를 가진 오브젝트를 찾습니다.
        // Physics.OverlapSphere는 Collider 배열을 반환합니다.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _data.targetSearchRadius);

        float closestDistance = Mathf.Infinity;
        Transform potentialTarget = null;

        foreach (var hitCollider in hitColliders)
        {
            // 자신은 타겟이 될 수 없음
            if (hitCollider.gameObject == _caster || hitCollider.gameObject == gameObject) continue;

            // 올바른 태그의 타겟만 고려
            if (hitCollider.CompareTag(_targetTag))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                // 가장 가까운 타겟을 찾음
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    potentialTarget = hitCollider.transform;
                }
            }
        }

        _currentTarget = potentialTarget;
    }

    void OnTriggerEnter(Collider other)
    {
        // 자기 자신이나 쏜 사람, 같은 편은 무시
        if (other.gameObject == _caster || (_caster != null && other.gameObject.layer == _caster.layer))
        {
            return;
        }

        // 공격해야 할 대상과 부딪혔는지 확인
        if (!string.IsNullOrEmpty(_targetTag) && other.CompareTag(_targetTag))
        {
            // 몬스터 대상
            if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
            {
                enemyHealth.TakeDamage(_data.damage);
                Debug.Log($"<color=yellow>{other.name}에게 {_data.damage}의 유도 데미지를 입혔습니다!</color>");
            }
            // 플레이어 대상
            else if (other.TryGetComponent<PlayerHealth>(out var playerHealth))
            {
                // 플레이어 체력 스크립트가 있다면 데미지 적용
                // playerHealth.TakeDamage(_data.damage); // 플레이어 데미지 로직 구현 필요
                Debug.Log($"<color=red>플레이어에게 {_data.damage}의 유도 데미지를 입혔습니다!</color>");
            }

            // 특수 효과 적용 로직
            ApplyOnHitEffects(other.gameObject);

            // 공격 성공 시 비활성화
            StopAllCoroutines();
            gameObject.SetActive(false);
            return;
        }

        // Trigger가 아닌 물리적 장애물(벽 등)과 부딪혔을 때 비활성화
        if (!other.isTrigger)
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }

    private void ApplyOnHitEffects(GameObject target)
    {
        if (_data != null && _data.onHitEffects != null && _data.onHitEffects.Count > 0)
        {
            if (target.TryGetComponent<CharacterStatus>(out var targetStatus))
            {
                foreach (var effect in _data.onHitEffects)
                {
                    if (effect != null)
                    {
                        effect.ApplyEffect(targetStatus);
                    }
                }
            }
        }
    }

    IEnumerator DeactivateAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }

    // 씬 뷰에서 탐색 반경을 시각적으로 보여주기 위한 함수 (개발용)
    void OnDrawGizmos()
    {
        if (_data != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _data.targetSearchRadius);
        }
    }
}