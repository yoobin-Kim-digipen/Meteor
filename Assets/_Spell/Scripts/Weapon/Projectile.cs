using UnityEngine;
using System.Collections;
public class Projectile : MonoBehaviour
{
    float speed;
    float lifetime;
    float damage;
    Rigidbody rb;

    public void Initialize(WeaponData weaponData)
    {
        this.speed = weaponData.projectileSpeed;
        this.lifetime = weaponData.projectileLifetime;
        this.damage = weaponData.damage;
    }

    void Awake()
    {
        Debug.Log(gameObject.name + " Awake! Trying to get Rigidbody.");
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(gameObject.name + " could NOT find Rigidbody component!");
        }
    }

    // OnEnable함수 <- start나 awake와 달리 오브젝트가 비활성화 되었다가 다시 활성화 될때도 호출됨
    void OnEnable()
    {
        rb.linearVelocity = transform.forward * speed;

        // 매개변수라는 코루틴(시간차를 두고 실행되는 함수)을 시작 / 발사체 비활성화 타이머
        // 그냥 그거임 날아가다가 뭐 아무 충돌없는데 몇초뒤에 뒤지셈 그런거
        StartCoroutine(DeactivateAfterTime(lifetime));
    }

    IEnumerator DeactivateAfterTime(float time) // 반환 타입: 코루틴 함수의 return 타입은 반드시 IEnumerator 여야 함.
    {
        // 함수실행 일시정지 return으로 빠져나가는것이 아님
        yield return new WaitForSeconds(time);

        // yield return 시간이 지난 후 setactive 실행(오브젝트 비활성화)후 종료
        gameObject.SetActive(false);
    }

    // 트리거충돌(collider가 겹쳤을때 알려줌)이 발생했을때 유니티 엔진이 호출
    void OnTriggerEnter(Collider other)
    {
        // EnemyHealth 스크립트를 가진 적과 부딪혔는지 확인
        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        StopAllCoroutines();
        gameObject.SetActive(false);
    }
}