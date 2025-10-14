using UnityEngine;

public abstract class PlayerBaseState // abstract -> 자식클래스 override 필수
{
    protected PlayerStateMachine _ctx; // Context (상태 머신의 주인)
    protected PlayerStateFactory _factory; // 상태들을 생성하는 팩토리

    public PlayerBaseState(PlayerStateMachine context, PlayerStateFactory factory)
    {
        _ctx = context;
        _factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void FixedUpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchStates();

    protected void HandleAirborneSwitchStates()
    {
        Player _player = _ctx.playerMovement;

        if (_player.canDoubleJump && _player.jumpQueued && !_player._hasDoubleJumped)
        {
            // 1. 점프 큐를 즉시 소비
            _player.jumpQueued = false;

            // 2. 더블 점프 기회를 사용했다고 표시
            _player._hasDoubleJumped = true;

            // 3. 더블 점프 힘 계산 및 적용
            float vy = Mathf.Sqrt(Mathf.Max(0f, -2f * Physics.gravity.y * _player.doubleJumpHeight));
            Vector3 v = _player.Rigidbody.linearVelocity;
            v.y = vy; // 현재 수직 속도를 덮어쓰기
            _player.Rigidbody.linearVelocity = v;

            // 점프 컷 등을 위한 변수들 초기화
            _player.jumpStartTime = Time.time;
        }
    }
}