using UnityEngine;
using System.Collections;

public class MonsterSuicideAttackState : MonsterBaseState
{
    public SuicideMonsterData SuicideData { get; private set; }
    private bool _isExploding = false; // 자폭 코루틴이 실행 중인지 확인하는 플래그

    public MonsterSuicideAttackState(MonsterStateMachine context, MonsterStateFactory factory) : base(context, factory) { }

    public override void EnterState()
    {
        SuicideData = _monster.monsterData as SuicideMonsterData;
        if (SuicideData == null)
        {
            // 잘못된 데이터가 들어오면 즉시 추적 상태로 복귀
            _ctx.SwitchState(_factory.Chase());
            return;
        }

        _monster.agent.isStopped = true;
        _isExploding = false;
    }

    public override void UpdateState()
    {
        // 이미 자폭 시퀀스가 시작되었다면, 새로운 시퀀스를 시작하지 않음
        if (_isExploding) return;

        // 플레이어가 범위를 벗어나면 다시 추적
        if (!_monster.IsPlayerInAttackRange())
        {
            _ctx.SwitchState(_factory.Chase());
            return;
        }

        // 자폭 시퀀스 시작
        _monster.StartCoroutine(ExplodeCoroutine());
    }

    public override void ExitState()
    {
        _monster.StopAllCoroutines(); // 상태를 나갈 때 모든 코루틴 정리
        _monster.agent.isStopped = false;
    }

    private IEnumerator ExplodeCoroutine()
    {
        _isExploding = true;

        // --- 1. 선딜레이 (Charge Time) ---
        Debug.Log($"<color=red>{_monster.name}가 자폭을 준비합니다! ({SuicideData.chargeTime}초)</color>");
        // (여기에 소리가 나는 이펙트/사운드 재생)
        yield return new WaitForSeconds(SuicideData.chargeTime);

        // --- 2. 실제 자폭 (Explosion) ---
        Debug.Log($"<color=red>BOOM!</color> {_monster.name}이(가) 자폭했습니다!");
        // (여기에 폭발 이펙트/사운드 재생)

        // 지정된 반경(explosionRadius) 내의 모든 콜라이더를 찾음
        Collider[] hits = Physics.OverlapSphere(
            _monster.transform.position,
            SuicideData.explosionRadius
        );

        bool playerHit = false; // 플레이어를 맞췄는지 확인하기 위한 플래그

        foreach (var hit in hits)
        {
            // 그 중에서 "Player" 태그를 가진 오브젝트를 찾음
            if (hit.CompareTag("Player"))
            {
                //  높이(돔형 판정) 체크
                float heightDifference = Mathf.Abs(hit.transform.position.y - _monster.transform.position.y);
                if (heightDifference <= SuicideData.explosionHeight)
                {
                    // 높이 조건까지 만족하면 공격 성공으로 간주
                    playerHit = true;
                    break;
                }
            }
        }

        // 최종 결과 로그 출력
        if (playerHit)
        {
            Debug.Log($"<color=orange>자폭 성공!</color> 플레이어가 폭발에 휘말렸습니다. (데미지: {SuicideData.damage})");
        }
        else
        {
            Debug.Log("자폭 실패! 플레이어가 범위 내에 없었습니다.");
        }

        // --- 3. 자폭 후 사망 처리 ---
        // 몬스터 자신을 비활성화 (오브젝트 풀로 반환)
        _monster.gameObject.SetActive(false);
    }
}