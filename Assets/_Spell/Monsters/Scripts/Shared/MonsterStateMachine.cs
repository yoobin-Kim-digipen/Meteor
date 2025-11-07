using UnityEngine;

public class MonsterStateMachine : MonoBehaviour
{
    public MonsterFSM MonsterFSM { get; private set; }

    private MonsterBaseState _currentState;
    private MonsterStateFactory _states;
    public MonsterBaseState CurrentState { get { return _currentState; } }
    void Awake()
    {
        MonsterFSM = GetComponent<MonsterFSM>();
    }
    public void SwitchToReturnState()
    {
        SwitchState(_states.Return());
    }
    public void Initialize(MonsterData data)
    {
        _states = new MonsterStateFactory(this);

        // 몬스터 타입에 따라 다른 시작 상태를 가짐
        if (MonsterFSM.aiType == AIType.LairGuardian)
        {
            _currentState = _states.Idle();
        }
        else // 야생 몬스터(Wanderer)는 바로 추적 시작
        {
            _currentState = _states.Chase();
        }
        _currentState.EnterState();
    }

    void Update()
    {
        if (_currentState != null)
        {
            _currentState.UpdateState();
        }
    }

    public void SwitchState(MonsterBaseState newState)
    {
        _currentState.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }
}