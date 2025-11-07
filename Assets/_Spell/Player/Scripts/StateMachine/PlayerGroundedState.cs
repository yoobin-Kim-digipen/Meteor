using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGroundedState : PlayerBaseState
{
    private Player _player;
    private float _groundedGraceTimer; // <--- 유예 시간을 셀 타이머 변수 추가
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory factory) : base(context, factory)
    {
        _player = _ctx.playerMovement; // Player 컴포넌트 참조
    }

    public override void EnterState()
    {
        //Debug.Log("Enter Grounded State");
        _player.jumpReleaseQueued = false;
        _groundedGraceTimer = _player.groundedGracePeriod;
    }

    public override void UpdateState()
    {
        // 매 프레임 상태 전환 조건을 확인
        CheckSwitchStates();
    }

    public override void FixedUpdateState()
    {
        // 공격 중일 때와 아닐 때의 로직을 분리
        if (_player.IsAttacking)
        {
            // 1. 공격 관리자에게 조준과 발사를 모두 위임
            if (_player.attackManager != null)
            {
                _player.attackManager.HandleAimAndAttack();
            }

            // 2. 공격 중에도 이동은 가능하도록 처리
            // 단, 회전은 AttackManager가 하므로 여기서는 회전 로직을 제외
            HandleGroundedMovement(applyRotation: false);
        }
        else
        {
            // 3. 공격 중이 아닐 때는 평소처럼 이동과 회전을 모두 처리
            HandleGroundedMovement(applyRotation: true);
        }
    }

    public override void ExitState()
    {

    }

    public override void CheckSwitchStates()
    {
        // 점프 조건은 최우선으로 확인
        if (_player.jumpQueued && _player.isGrounded)
        {
            _ctx.SwitchState(_factory.Jump());
            return;
        }

        if (_player.isGrounded)
        {
            // 땅에 붙어있다면, 타이머를 계속 최대로 유지
            _groundedGraceTimer = _player.groundedGracePeriod;
        }
        else
        {
            // 땅에서 떨어졌다면, 타이머를 감소시키기 시작
            _groundedGraceTimer -= Time.deltaTime;

            // 타이머가 0 이하로 떨어져야만 '진짜 추락'으로 간주하고 Fall 상태로 전환
            if (_groundedGraceTimer <= 0f)
            {
                _ctx.SwitchState(_factory.Fall());
            }
        }
    }
    private void HandleGroundedMovement(bool applyRotation)
    {
        // 1. StatManager로부터 시간 보정 값을 가져옵니다 (평소: 1, 슬로우: 5 등)
        float timeMultiplier = _player.TimeScaleMultiplier;
        
        float dt = Time.fixedDeltaTime;
        Vector3 v = _player.Rigidbody.linearVelocity;

        Vector3 planar = new Vector3(v.x, 0f, v.z);

        // 2. 목표 속도에 시간 보정 값을 곱해줍니다.
        float targetSpeed = _player.moveSpeed * Mathf.Clamp01(_player.moveDir.magnitude) * timeMultiplier;
        Vector3 wishDir = (_player.moveDir.sqrMagnitude > 1e-6f) ? _player.moveDir.normalized : Vector3.zero;

        if (wishDir != Vector3.zero)
        {
            float curAlong = Vector3.Dot(planar, wishDir);
            float addNeeded = targetSpeed - curAlong;

            if (addNeeded > 0f)
            {
                // 3. 가속 및 감속 계산에도 시간 보정 값을 곱해줍니다.
                float add = Mathf.Min(_player.acceleration * dt * timeMultiplier, addNeeded);
                planar += wishDir * add;
            }
            else
            {
                float reduce = Mathf.Min(_player.deceleration * dt * timeMultiplier, -addNeeded);
                planar += wishDir * (-reduce);
            }

            if (planar.magnitude > targetSpeed)
            {
                float over = planar.magnitude - targetSpeed;
                float cut = Mathf.Min(_player.deceleration * dt * timeMultiplier, over);
                planar = planar.normalized * (planar.magnitude - cut);
            }
        }
        else
        {
            // 입력이 없을 때의 감속에도 보정 값을 적용합니다.
            planar = Vector3.MoveTowards(planar, Vector3.zero, _player.deceleration * dt * timeMultiplier);
        }

        v.x = planar.x;
        v.z = planar.z;
        _player.Rigidbody.linearVelocity = v;

        if (applyRotation && _player.wantRotate)
        {
            // 참고: 캐릭터 회전도 느려지는 현상을 막으려면,
            // Player.cs의 faceCameraYaw() 함수 내부에서도 turnSpeed에 timeMultiplier를 곱해주어야 합니다.
            _player.faceCameraYaw();
        }
    }
}