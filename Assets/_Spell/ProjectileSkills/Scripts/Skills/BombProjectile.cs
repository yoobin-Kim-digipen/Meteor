using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BombProjectile : Skill // Skill을 상속
{
    private float _speed, _lifetime, _damage;
    private Rigidbody _rb;
    private string _targetTag;
    private GameObject _caster;
    private BombProjectileSkillData _data;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void Activate(GameObject caster, SkillData data)
    {
        _caster = caster;
        if (data is BombProjectileSkillData projData)
        {
            this._data = projData;

            // _data를 통해 능력치 설정
            _speed = this._data.speed;
            _lifetime = this._data.lifetime;
            _damage = this._data.damage;

            if (caster.CompareTag("Player")) _targetTag = "Enemy";
            else if (caster.CompareTag("Enemy")) _targetTag = "Player";

            _rb.linearVelocity = transform.forward * _speed;
            StartCoroutine(DeactivateAfterTime(_lifetime));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 자기 자신이나 쏜 사람, 같은 편(동일한 레이어)은 무시
        if (other.gameObject == _caster || (_caster != null && other.gameObject.layer == _caster.layer))
        {
            return;
        }

        // 유효한 타겟(적)에 맞았거나, Trigger가 아닌 장애물(벽 등)과 부딪혔는지 확인
        bool hitValidTarget = !string.IsNullOrEmpty(_targetTag) && other.CompareTag(_targetTag);
        bool hitObstacle = !other.isTrigger;

        if (hitValidTarget || hitObstacle)
        {
            // 폭발 처리
            HandleExplosion(transform.position);

            // 투사체 비활성화
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }

    private void HandleExplosion(Vector3 explosionPosition)
    {
        // 1. 폭발 파티클 효과 생성
        // SkillData에 explosionPrefab이 할당되어 있다면, 해당 위치에 생성
        if (_data != null && _data.explosionPrefab != null)
        {
            // 폭발 이펙트 프리팹은 일정 시간 후 자동으로 파괴되는 스크립트를 포함하는 것이 좋습니다.
            Instantiate(_data.explosionPrefab, explosionPosition, Quaternion.identity);
        }

        // 2. 광역 데미지 처리
        // SkillData의 explosionRadius가 0보다 크면, 주변에 데미지를 줌
        if (_data != null && _data.explosionRadius > 0)
        {
            // 지정된 반경 내의 모든 콜라이더를 가져옴
            Collider[] colliders = Physics.OverlapSphere(explosionPosition, _data.explosionRadius);

            foreach (var hitCollider in colliders)
            {
                // 공격해야 할 대상 태그를 가진 오브젝트인지 확인
                if (!string.IsNullOrEmpty(_targetTag) && hitCollider.CompareTag(_targetTag))
                {
                    // 체력 컴포넌트가 있는지 확인
                    if (hitCollider.TryGetComponent<EnemyHealth>(out var enemyHealth))
                    {
                        enemyHealth.TakeDamage(_damage);
                    }
                    else if (hitCollider.TryGetComponent<PlayerHealth>(out var playerHealth))
                    {
                        // 플레이어에게 데미지를 주는 로직 (필요시 구현)
                    }

                    // On-Hit 특수 효과도 광역으로 적용
                    ApplyOnHitEffects(hitCollider.gameObject);
                }
            }
        }
    }
    
    private void ApplyOnHitEffects(GameObject target)
    {
        // if (_data != null && _data.onHitEffects != null && _data.onHitEffects.Count > 0)
        // {
        //     if (target.TryGetComponent<CharacterStatus>(out var targetStatus))
        //     {
        //         foreach (var effect in _data.onHitEffects)
        //         {
        //             if (effect != null)
        //             {
        //                 effect.ApplyEffect(targetStatus);
        //             }
        //         }
        //     }
        // }
    }

    IEnumerator DeactivateAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        
        // 시간이 다 되어 사라질 때도 폭발 효과를 낼 수 있음.
        // 필요하다면 이 부분을 비활성화
        HandleExplosion(transform.position);
        
        gameObject.SetActive(false);
    }
}