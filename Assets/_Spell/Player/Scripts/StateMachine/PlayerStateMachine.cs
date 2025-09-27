using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    // �ܺο��� ������ ������Ʈ��
    [Header("Components")]
    public Player playerMovement; // Player.cs
    public PlayerAttackManager playerAttack; // PlayerAttackManager.cs
    public Animator animator; // �ִϸ����� (���߿� ���)

    // ���� �ӽ� ���� ����
    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    // �ܺο��� �б� �������� ���� ���� Ȯ�� (������)
    public PlayerBaseState CurrentState { get { return _currentState; } private set { _currentState = value; } }

    void Awake()
    {
        // ������Ʈ �ڵ� �Ҵ�
        playerMovement = GetComponent<Player>();
        playerAttack = GetComponent<PlayerAttackManager>();
        animator = GetComponentInChildren<Animator>();

        // ���� ���丮�� �ʱ� ���� ����
        _states = new PlayerStateFactory(this);
        CurrentState = _states.Grounded(); // ������ Grounded ����
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

    // ���� ��ȯ �Լ�
    public void SwitchState(PlayerBaseState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}