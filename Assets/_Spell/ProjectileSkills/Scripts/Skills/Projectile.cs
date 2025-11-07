using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : Skill // Skill을 상속
{
    private float _speed, _lifetime, _damage;
    private Rigidbody _rb;
    private string _targetTag;
    private GameObject _caster;
    private ProjectileSkillData _data;

    private bool _isPlayerProjectile = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void Activate(GameObject caster, SkillData data)
    {
        _caster = caster;
        if (data is ProjectileSkillData projData)
        {
            this._data = projData;
            _speed = _data.speed;
            _lifetime = _data.lifetime;
            _damage = _data.damage;

            if (caster.CompareTag("Player"))
            {
                _targetTag = "Enemy";
                _isPlayerProjectile = true; // 플레이어의 발사체임을 기억
            }
            else if (caster.CompareTag("Enemy"))
            {
                _targetTag = "Player";
                _isPlayerProjectile = false;
            }

            // Activate 시점에는 속도를 설정하지 않고, FixedUpdate에서 계속 갱신
            StartCoroutine(DeactivateAfterTime(_lifetime));
        }
    }

    void FixedUpdate()
    {
        float timeMultiplier = 1.0f;

        // 이 발사체가 플레이어의 것이라면, StatManager로부터 시간 보정 값을 가져옴
        if (_isPlayerProjectile && StatManager.Instance != null)
        {
            timeMultiplier = StatManager.Instance.PlayerTimeScaleMultiplier;
        }

        // 시간 보정 값을 적용하여 속도를 매 프레임 유지
        _rb.linearVelocity = transform.forward * _speed * timeMultiplier;
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
                // 있다면, 데미지를 입힘!
                enemyHealth.TakeDamage(_damage);
                //Debug.Log($"<color=yellow>{other.name}에게 {_damage}의 데미지를 입혔습니다!</color>");
            }

            // 플레이어 대상
            else
            {
                //Debug.Log($"<color=yellow>{other.name}이(가) {_damage}의 데미지를 입었습니다!</color>");
            }

            // 특수 효과 적용 로직
            // 데이터가 있고, onHitEffects 리스트에 효과가 하나라도 들어있다면
            if (_data != null && _data.onHitEffects.Count > 0)
            {
                // 부딪힌 대상에게 CharacterStatus 컴포넌트가 있는지 확인
                if (other.TryGetComponent<CharacterStatus>(out var targetStatus))
                {
                    // 이 발사체가 가진 모든 On-Hit 효과들을 순서대로 적용
                    foreach (var effect in _data.onHitEffects)
                    {
                        if (effect != null)
                        {
                            // 각 효과에게 적용 명령
                            effect.ApplyEffect(targetStatus);
                        }
                    }
                }
            }

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

    IEnumerator DeactivateAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}