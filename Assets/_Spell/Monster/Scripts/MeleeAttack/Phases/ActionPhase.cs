using UnityEngine;
public class ActionPhase : IAttackPhase
{
    private MonsterMeleeAttackState _superState;
    private bool _hasDealtDamage; // 데미지를 한 번만 주도록 체크하는 플래그

    public ActionPhase(MonsterMeleeAttackState superState)
    {
        _superState = superState;
    }

    public void EnterPhase()
    {
        _hasDealtDamage = false; // 단계 진입 시 플래그 초기화

        // (애니메이션) 실제 공격 애니메이션 재생
        // _superState.Monster.Animator.SetTrigger("AttackAction");
    }

    public void UpdatePhase()
    {
        // 이 단계에서는 진입 후 첫 프레임에 딱 한 번만 데미지 판정을 수행
        if (!_hasDealtDamage)
        {
            PerformMeleeAttack();
            _hasDealtDamage = true;
        }

        // 애니메이션이 없는 현재는, 판정 후 즉시 회복 단계로 전환.
        // (나중에 애니메이션 길이에 맞춰 전환 타이밍을 조절하면 더 자연스러워짐)
        _superState.SwitchPhase(typeof(RecoveryPhase));
    }

    public void ExitPhase() { }

    private void PerformMeleeAttack()
    {
        if (_superState.Monster.target == null) return;

        bool isDistanceOK = _superState.Monster.IsPlayerInAttackRange();

        // 플레이어를 향하는 방향 벡터에서 Y(높이) 값을 제거하여 수평 방향만 계산
        Vector3 directionToPlayer3D = _superState.Monster.target.position - _superState.Monster.transform.position;
        Vector3 directionToPlayer2D = new Vector3(directionToPlayer3D.x, 0, directionToPlayer3D.z);

        // 방향 벡터가 0이 아닐 때만 각도를 계산
        float angle = 999f; // 기본값을 큰 값으로 설정
        if (directionToPlayer2D.sqrMagnitude > 0.001f)
        {
            angle = Vector3.Angle(_superState.Monster.transform.forward, directionToPlayer2D.normalized);
        }
        bool isAngleOK = angle <= _superState.MeleeData.attackAngle / 2;

        // 몬스터와 플레이어 사이의 순수한 수직 높이 차이를 계산
        float heightDifference = Mathf.Abs(directionToPlayer3D.y);
        bool isHeightOK = heightDifference <= _superState.MeleeData.attackHeight;

        // 3가지 조건(거리, 각도, 높이)을 모두 만족할 때만 공격이 성공
        if (isDistanceOK && isAngleOK && isHeightOK)
        {
            // Console 창에 주황색으로 성공 로그를 출력
            Debug.Log($"<color=orange>{_superState.Monster.name}의 근접 공격 성공!</color> " +
                      $"(각도: {angle:F1}°, 높이차: {heightDifference:F1}m)");

            // 나중에 이 아래에 IDamageable을 이용한 실제 데미지 코드를 추가

        }
        else
        {
            // 어떤 조건 때문에 실패했는지 상세한 로그를 출력
            Debug.Log($"{_superState.Monster.name}의 헛스윙! " +
                      $"(거리OK: {isDistanceOK}, 각도OK: {isAngleOK}, 높이OK: {isHeightOK})");
        }
    }
}
