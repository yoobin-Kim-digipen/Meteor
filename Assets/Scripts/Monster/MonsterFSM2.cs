using UnityEngine;
using UnityEngine.AI;

public class MonsterFSM2 : MonoBehaviour
{
    public enum MonsterState
    {
        Wander,
        Chase
    }
    public float detectionRadius = 5f;
    public Transform target; // Wizard Transform

    private NavMeshAgent agent;
    private MonsterState currentState = MonsterState.Wander;
    private Vector3 wanderOrigin;
    private float wanderRadius = 8f;
    private float waitTime = 1.5f;
    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        wanderOrigin = transform.position;
        SetRandomDestination();
    }

    void Update()
    {
        switch (currentState)
        {
            case MonsterState.Wander:
                WanderUpdate();
                CheckForTarget();
                break;
            case MonsterState.Chase:
                ChaseUpdate();
                break;
        }
    }

    void CheckForTarget()
    {
        if (target && Vector3.Distance(transform.position, target.position) < detectionRadius)
        {
            currentState = MonsterState.Chase;
        }
    }

    void WanderUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                SetRandomDestination();
                waitTimer = 0f;
            }
        }
        
        if (currentState == MonsterState.Chase && Vector3.Distance(transform.position, target.position) > detectionRadius)
        {
            currentState = MonsterState.Wander;
            SetRandomDestination();
        }
    }

    void ChaseUpdate()
    {
        if (target)
        {
            agent.SetDestination(target.position);
            if (Vector3.Distance(transform.position, target.position) > detectionRadius)
            {
                currentState = MonsterState.Wander;
                SetRandomDestination();
            }
        }
    }

    void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius + wanderOrigin;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}

