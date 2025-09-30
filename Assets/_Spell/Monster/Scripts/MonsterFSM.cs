using UnityEngine;
using UnityEngine.AI;

// 몬스터 프리팹에 필요한 컴포넌트들을 자동으로 추가해주고, 삭제하지 못하게 막는 어트리뷰트
[RequireComponent(typeof(NavMeshAgent), typeof(MonsterStateMachine), typeof(EnemyHealth))]
public class MonsterFSM : MonoBehaviour
{
    [Header("AI Data")]
    public MonsterData monsterData;
    public Transform target;

    public Collider targetCollider { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public MonsterStateMachine stateMachine { get; private set; }
    public EnemyHealth health { get; private set; }
    public Animator animator { get; private set; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        stateMachine = GetComponent<MonsterStateMachine>();
        health = GetComponent<EnemyHealth>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Initialize(MonsterData data, Transform playerTarget)
    {
        this.monsterData = data;
        this.target = playerTarget;

        agent.speed = monsterData.speed;
        // 내가 가진 monsterData를 Health 컴포넌트에게 전달하여 초기화시킨다.
        health.Initialize(monsterData);
        stateMachine.Initialize(monsterData);
    }

    // 오브젝트가 활성화될 때마다 호출되는 함수
    void OnEnable()
    {
        // NavMeshAgent가 비활성화 상태일 수 있으므로 안전하게 다시 켜줌.
        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position); // 이전 경로와 상태를 완전히 초기화
        }
    }

    void OnDisable()
    {
        if (agent != null && agent.enabled)
        {
            // 경로 계산을 멈춰서 불필요한 연산을 막음
            agent.ResetPath();
            agent.enabled = false;
        }
    }

    public bool IsPlayerInAttackRange()
    {
        // target이나 monsterData가 할당되지 않은 예외 상황을 안전하게 처리.
        if (target == null || monsterData == null) return false;

        // 몬스터의 현재 위치와 타겟의 위치 사이의 거리를 계산하고,
        // 그 거리가 monsterData에 정의된 attackRange보다 작거나 같은지 확인.
        return Vector3.Distance(transform.position, target.position) <= monsterData.attackRange;
    }

    // 디버깅용: 씬(Scene) 뷰에서 몬스터의 공격 범위를 시각적으로 보여줌
    void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterData.attackRange);
    }
}

