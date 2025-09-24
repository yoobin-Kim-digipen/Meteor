using UnityEngine;
using UnityEngine.AI;

public class MonsterFSM : MonoBehaviour
{
    public Transform target; // 플레이어(위저드) Transfrom
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // 오브젝트가 활성화될 때마다 호출되는 함수
    void OnEnable()
    {
        // NavMeshAgent가 비활성화 상태일 수 있으므로 안전하게 다시 켜줌.
        if (agent != null)
        {
            agent.enabled = true;

            // 이전 경로와 상태를 완전히 초기화
            agent.Warp(transform.position);

            // 활성화된 직후 바로 목표를 향해 가도록 설정
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }

    void Update()
    {
        if (target && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
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
}

