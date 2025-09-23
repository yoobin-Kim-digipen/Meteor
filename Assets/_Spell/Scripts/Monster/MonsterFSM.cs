using UnityEngine;
using UnityEngine.AI;

public class MonsterFSM : MonoBehaviour
{
    public Transform target; // 플레이어(위저드) Transfrom
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (target)
        {
            agent.SetDestination(target.position);
        }
    }
}

