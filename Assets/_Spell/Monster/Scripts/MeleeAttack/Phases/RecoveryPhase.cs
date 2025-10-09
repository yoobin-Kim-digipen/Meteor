using UnityEngine;
public class RecoveryPhase : IAttackPhase
{
    private MonsterMeleeAttackState _superState;
    private float _recoveryTimer;

    public RecoveryPhase(MonsterMeleeAttackState superState)
    {
        _superState = superState;
    }

    public void EnterPhase()
    {
        // 쿨타임 시간을 데이터에서 가져와 타이머 설정
        _recoveryTimer = _superState.MeleeData.attackCooldown;

        // (애니메이션) 공격 후 자세를 가다듬는 애니메이션 재생
        // _superState.Monster.Animator.SetTrigger("Recovery");
    }

    public void UpdatePhase()
    {
        _recoveryTimer -= Time.deltaTime;
        if (_recoveryTimer <= 0)
        {
            // 후딜레이(쿨타임)가 모두 끝나면, 공격 사이클이 종료됨.
            // 상위 상태에 '추적 상태로 돌아가라'고 요청.
            _superState.BackToChase();
        }
    }

    public void ExitPhase() { }
}