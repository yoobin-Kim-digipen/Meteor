using UnityEngine;
using UnityEngine.AI;

// 몬스터 프리팹에 필요한 컴포넌트들을 자동으로 추가해주고, 삭제하지 못하게 막는 어트리뷰트
[RequireComponent(typeof(NavMeshAgent), typeof(MonsterStateMachine), typeof(EnemyHealth))]
public class MonsterFSM : MonoBehaviour, IStats
{
    [Header("AI Data")]
    public MonsterData monsterData;
    public Transform target;

    public Collider targetCollider { get; private set; }
    public NavMeshAgent agent { get; private set; }
    public float MoveSpeed
    {
        // 몬스터의 이동 속도는 NavMeshAgent의 speed 속성이 담당
        get { return agent.speed; }
        set { agent.speed = value; }
    }

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
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            // 경로 계산을 멈춰서 불필요한 연산을 막음
            agent.ResetPath();
            agent.enabled = false;
        }
        else if (agent != null) // NavMesh 위에 있지 않더라도, Agent 자체는 비활성화
        {
            agent.enabled = false;
        }
    }

    public bool IsPlayerInAttackRange()
    {
        if (target == null || monsterData == null) return false;

        Vector3 monsterPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPos = new Vector3(target.position.x, 0, target.position.z);

        if (Vector3.Distance(monsterPos, targetPos) > monsterData.attackRange)
        {
            return false; // 범위를 벗어났음
        }
        else
        {
            return true; // 범위 안에 있음
        }
    }

    // 디버깅용: 씬(Scene) 뷰에서 몬스터의 공격 범위를 시각적으로 보여줌
    void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterData.attackRange);
    }


    // ---------------------------------------------------- debug ------------------------------------------
    void OnDrawGizmos()
    {
        // 데이터가 없거나, 상태 머신이 아직 준비되지 않았으면 아무것도 그리지 않음
        if (monsterData == null || stateMachine == null || stateMachine.CurrentState == null)
        {
            return;
        }

        // --- 1. 기본 공격 범위 (원) ---
        // 항상 표시되는 기본 attackRange 원 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterData.attackRange);

        // ▼▼▼▼▼ [핵심] 부채꼴 공격 범위 그리기 ▼▼▼▼▼

        // --- 2. 근접 공격 범위 (부채꼴) ---
        // 현재 상태가 'MonsterAttackState'일 때만 부채꼴을 그림
        if (stateMachine.CurrentState is MonsterMeleeAttackState)
        {
            // 데이터가 MeleeMonsterData 타입인지 확인
            if (monsterData is MeleeMonsterData meleeData)
            {
                // 부채꼴의 색상을 주황색으로 설정
                Gizmos.color = new Color(1.0f, 0.5f, 0.0f); // 주황색

                // 부채꼴의 중심선(정면 방향) 그리기
                Vector3 forward = transform.forward * meleeData.attackRange;
                Gizmos.DrawRay(transform.position, forward);

                // 부채꼴의 양쪽 끝 경계선 계산
                float halfAngle = meleeData.attackAngle / 2.0f;
                Quaternion leftRayRotation = Quaternion.AngleAxis(-halfAngle, Vector3.up);
                Quaternion rightRayRotation = Quaternion.AngleAxis(halfAngle, Vector3.up);

                Vector3 leftRayDirection = leftRayRotation * transform.forward;
                Vector3 rightRayDirection = rightRayRotation * transform.forward;

                // 양쪽 경계선 그리기
                Gizmos.DrawRay(transform.position, leftRayDirection * meleeData.attackRange);
                Gizmos.DrawRay(transform.position, rightRayDirection * meleeData.attackRange);

#if UNITY_EDITOR // Unity 에디터에서만 실행되도록 하는 전처리기
                // 부채꼴의 호(arc) 그리기 (더 예쁘게 보이기 위함)
                UnityEditor.Handles.color = new Color(1.0f, 0.5f, 0.0f, 0.1f); // 반투명 주황색
                UnityEditor.Handles.DrawSolidArc(
                    transform.position,     // 원의 중심
                    Vector3.up,             // 회전 축 (Y축)
                    leftRayDirection,       // 시작 방향
                    meleeData.attackAngle,  // 총 각도
                    meleeData.attackRange   // 반지름
                );
#endif
            }
        }
        else if (stateMachine.CurrentState is MonsterSuicideAttackState)
        {
            // --- 자폭 공격(Suicide) 범위 (돔/구체) ---
            if (monsterData is SuicideMonsterData suicideData)
            {
                // 자폭 준비 중임을 나타내기 위해 색상을 강렬한 빨간색으로 설정
                Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f); // 반투명 빨간색

                // 1. 바닥에 깔리는 원 그리기
                Gizmos.DrawSphere(transform.position, suicideData.explosionRadius);

                // 2. 돔 형태를 표현하기 위해 상반구 와이어프레임 그리기
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, suicideData.explosionRadius);

                // 더 정확한 돔 형태를 원한다면 Handles API를 사용할 수 있음
#if UNITY_EDITOR
                // 반구(Hemisphere)를 그려서 돔 형태를 완벽하게 시각화
                UnityEditor.Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.1f);
                UnityEditor.Handles.DrawSolidDisc(transform.position, Vector3.up, suicideData.explosionRadius); // 바닥 원 채우기
                UnityEditor.Handles.DrawWireArc(transform.position, Vector3.right, Vector3.forward, 180, suicideData.explosionRadius);
                UnityEditor.Handles.DrawWireArc(transform.position, Vector3.forward, Vector3.left, 180, suicideData.explosionRadius);
#endif
            }
        }
    }
}

