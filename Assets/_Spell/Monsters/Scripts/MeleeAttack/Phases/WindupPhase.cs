using UnityEngine;

// 공격 준비 (선딜레이) 단계
public class WindupPhase : IAttackPhase
{
    private MonsterMeleeAttackState _superState; // 이 단계를 관리하는 상위 상태
    private float _windupTimer;

    // 생성자: 상위 상태에 대한 참조를 받아 저장
    public WindupPhase(MonsterMeleeAttackState superState)
    {
        _superState = superState;
    }

    public void EnterPhase()
    {
        // 선딜레이 시간을 데이터에서 가져와 타이머 설정
        _windupTimer = _superState.MeleeData.attackWindupTime;

        // (애니메이션) 공격 준비 애니메이션 재생
        // _superState.Monster.Animator.SetTrigger("Windup");

        // 공격 방향을 이 단계에서 한 번만 고정
        if (_superState.Monster.target != null)
        {
            Vector3 lookDirection = (_superState.Monster.target.position - _superState.Monster.transform.position);
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                _superState.Monster.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    public void UpdatePhase()
    {
        // 이 단계에서 피격당하면 공격이 캔슬되고 'Stunned' 상태로 전환되는 로직 추가 가능
        _windupTimer -= Time.deltaTime;
        if (_windupTimer <= 0)
        {
            // 선딜레이가 끝나면 '실제 공격' 단계로 전환하도록 상위 상태에 요청
            _superState.SwitchPhase(typeof(ActionPhase));
        }
    }

    public void ExitPhase()
    {
        // 이 단계를 빠져나갈 때 특별히 할 작업은 없음
    }
}