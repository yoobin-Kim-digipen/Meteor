using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    // 외부에서 참조할 컴포넌트들
    [Header("Components")]
    public Player playerMovement; // Player.cs
    public PlayerAttackManager playerAttack; // PlayerAttackManager.cs
    public Animator animator; // 애니메이터 (나중에 사용)

    // 상태 머신 관련 변수
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    // 외부에서 읽기 전용으로 현재 상태 확인 (디버깅용)
    public PlayerBaseState CurrentState { get { return _currentState; } private set { _currentState = value; } }

    void Awake()
    {
        // 컴포넌트 자동 할당
        playerMovement = GetComponent<Player>();
        playerAttack = GetComponent<PlayerAttackManager>();
        animator = GetComponentInChildren<Animator>();

        // 상태 팩토리와 초기 상태 설정
        _states = new PlayerStateFactory(this);
        CurrentState = _states.Grounded(); // 시작은 Grounded 상태
        CurrentState.EnterState();
    }

    public void Update()
    {
        CurrentState.UpdateState();
    }

    public void FixedUpdate()
    {
        CurrentState.FixedUpdateState();
    }

    // 상태 전환 함수
    public void SwitchState(PlayerBaseState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}