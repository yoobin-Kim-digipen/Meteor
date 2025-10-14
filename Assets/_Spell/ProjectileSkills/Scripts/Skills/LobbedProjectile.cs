using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class LobbedProjectile : Skill
{
    private LobbedSkillData _data;
    private GameObject _caster;
    private string _targetTag;

    public override void Activate(GameObject caster, SkillData data)
    {
        _caster = caster;
        _data = data as LobbedSkillData;
        if (caster.CompareTag("Player")) _targetTag = "Enemy";
        else if (caster.CompareTag("Enemy")) _targetTag = "Player";
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        if (other == _caster || (_caster != null && other.layer == _caster.layer)) return;

        Explode();
        gameObject.SetActive(false);
    }
    private void Explode()
    {
        if (_data == null) return;

        ObjectPooler.Instance.GetFromPool("ExplosionEffect", transform.position, Quaternion.identity);

        // 폭발 반경 내의 모든 콜라이더를 찾음
        Collider[] hits = Physics.OverlapSphere(transform.position, _data.explosionRadius);
        foreach (var hit in hits)
        {
            // 그 중에서 공격 대상 태그와 일치하는 것을 찾음
            if (hit.CompareTag(_targetTag))
            {
                // --- 1. 데미지 처리 (로그) ---
                Debug.Log($"<color=yellow>{hit.name}이(가) 폭발에 휘말려 {_data.damage}의 데미지를 입었습니다!</color>");
 
                // 부딪힌 대상에게 CharacterStatus 컴포넌트가 있는지 확인
                if (hit.TryGetComponent<CharacterStatus>(out var status))
                {
                    // 이 스킬이 가진 모든 특수 효과를 순서대로 적용
                    foreach (var effect in _data.onHitEffects)
                    {
                        effect?.ApplyEffect(status);
                    }
                }

                // 부딪힌 대상이 Player 컴포넌트를 가지고 있는지 확인
                if (hit.TryGetComponent<Player>(out var player))
                {
                    // 폭발 중심에서 플레이어를 향하는 방향으로 밀어냄
                    Vector3 knockbackDir = (hit.transform.position - transform.position).normalized;

                    // 살짝 위로 띄우는 효과를 주기 위해 Y값을 보정
                    knockbackDir.y = Mathf.Max(knockbackDir.y, 0.5f);

                    player.ApplyKnockback(knockbackDir, _data.knockbackForce);
                    Debug.Log($"<color=lightblue>{hit.name}에게 넉백이 적용됩니다.</color>");
                }
            }
        }
    }
    void OnDrawGizmos()
    {
        // 데이터가 할당되지 않았으면 그리지 않음
        if (_data == null) return;

        // 폭발 범위를 나타내는 붉은색 와이어프레임 구를 그립니다.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _data.explosionRadius);
    }
}