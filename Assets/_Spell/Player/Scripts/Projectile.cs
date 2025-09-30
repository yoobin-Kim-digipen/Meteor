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

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void Activate(GameObject caster, SkillData data)
    {
        _caster = caster;
        if (data is ProjectileSkillData projData)
        {
            _speed = projData.speed;
            _lifetime = projData.lifetime;
            _damage = projData.damage;

            if (caster.CompareTag("Player")) _targetTag = "Enemy";
            else if (caster.CompareTag("Enemy")) _targetTag = "Player";

            _rb.linearVelocity = transform.forward * _speed;
            StartCoroutine(DeactivateAfterTime(_lifetime));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // 자기 자신이나 쏜 사람과 부딪히면 무시
        if (other.gameObject == _caster) return;

        // 공격해야 할 대상과 부딪혔는지 확인
        if (!string.IsNullOrEmpty(_targetTag) && other.CompareTag(_targetTag))
        {
            // 대상에게 EnemyHealth가 있다면 데미지 주기
            if (other.TryGetComponent<EnemyHealth>(out var enemy))
            {
                enemy.TakeDamage(_damage);
            }
            // 대상에게 PlayerHealth가 있다면 데미지 주기 (나중에 추가)
            // if (other.TryGetComponent<PlayerHealth>(out var player)) player.TakeDamage(_damage);
        }

        // 같은 편과 부딪히는 경우 무시 (예: 플레이어가 쏜 총알이 다른 플레이어 스킬에 닿는 경우)
        if (_caster != null && other.gameObject.layer == _caster.layer) return;

        // 그 외의 것(벽 등)에 부딪히면 사라짐
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    IEnumerator DeactivateAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}