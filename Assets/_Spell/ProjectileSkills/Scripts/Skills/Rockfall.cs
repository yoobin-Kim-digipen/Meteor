using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Rockfall : Skill
{
    private RockfallSkillData _data;
    private GameObject _caster;
    private string _targetTag;

    public override void Activate(GameObject caster, SkillData data)
    {
        _caster = caster;
        _data = data as RockfallSkillData;

        // Rigidbody의 속도를 0으로 만들어 자유 낙하 준비
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // OnCollisionEnter: 물리적 충돌이 발생했을 때 호출
    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // 1. 자기 자신이나 쏜 사람과 부딪히면 아무것도 하지 않고 통과
        if (other == _caster) return;

        // 2. 같은 편(같은 레이어)과 부딪히는 경우도 무시
        if (_caster != null && other.layer == _caster.layer) return;

        // 3. 부딪힌 대상이 '공격 대상'인지 먼저 확인
        if (other.TryGetComponent<EnemyHealth>(out var enemyHealth))
        {
            // 공격 대상이 맞다면, 데미지를 줌
            enemyHealth.TakeDamage(_data.damage);
            Debug.Log($"<color=yellow>{other.name}에게 낙석이 명중! {_data.damage}의 데미지!</color>");
        }

        gameObject.SetActive(false);
    }
}