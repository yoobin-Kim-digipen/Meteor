using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private float maxhealth; // 현재 체력은 내부에서만 관리
    public float currentHealth { get; private set; }

    // 오브젝트가 풀에서 나와 활성화될 때마다 호출됨
    public void Initialize(MonsterData data)
    {
        // MonsterData로부터 최대 체력 정보를 받아와 설정
        maxhealth = data.maxHealth;
        currentHealth = maxhealth;
    }

    void OnEnable()
    {
        currentHealth = maxhealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= StatManager.Instance.CalculateFinalDamage((int)damage, 20); // 몬스터는 방어력 20으로 가정
        if (currentHealth < 0) currentHealth = 0;
        if (currentHealth == 0)
        {
            StatManager.Instance.GainExperience(10); // 예시로 10 경험치 획득
            Die();
        }
    }

    private void Die()
    {
        // ex) 사망 상태로 전환하거나 사망 애니메이션/이펙트 처리
        // 예: GetComponent<MonsterFSM>().StateMachine.SwitchState(_factory.Dead());
        // 지금은 간단하게 오브젝트 풀로 반환
        gameObject.SetActive(false);
    }
}