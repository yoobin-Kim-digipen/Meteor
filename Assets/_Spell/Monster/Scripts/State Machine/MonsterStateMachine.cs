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

    public void Initialize(MonsterData data)
    {
        // 팩토리를 생성하고, 시작 상태를 'Chase'로 설정합니다.
        _states = new MonsterStateFactory(this);
        _currentState = _states.Chase();
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