using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : Skill // Skill을 상속
{
    private float _speed, _lifetime;
    public float Damage { get; private set; }
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
            Damage = projData.damage;

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

        // 같은 편과 부딪히는 경우 무시
        if (_caster != null && other.gameObject.layer == _caster.layer) return;

        // 공격해야 할 대상과 부딪혔는지 확인
        if (!string.IsNullOrEmpty(_targetTag) && other.CompareTag(_targetTag))
        {
            if (other.TryGetComponent<EnemyHealth>(out var enemy))
            {
                enemy.TakeDamage(Damage);
            }
            if(other.TryGetComponent<PlayerHealth>(out var player)) player.TakeDamage(Damage);

            // 공격 성공 시 사라짐
            StopAllCoroutines();
            gameObject.SetActive(false);
            return;
        }

        // Trigger가 아닌 물리적 장애물(벽 등)과 부딪혔을 때만 사라짐
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