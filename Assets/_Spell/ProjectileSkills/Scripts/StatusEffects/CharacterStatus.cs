using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterStatus : MonoBehaviour
{
    // 이 캐릭터의 능력치 정보 (Player 또는 MonsterFSM)
    private IStats _stats;

    // 효과에 걸리기 전의 원래 이동 속도를 저장
    private float _originalMoveSpeed;

    // 현재 적용 중인 효과 코루틴들을 관리 (효과 갱신/중첩을 위해)
    private Dictionary<System.Type, Coroutine> _activeCoroutines = new Dictionary<System.Type, Coroutine>();

    // 게임 시작 시, 자신의 능력치 정보를 찾아 저장
    void Awake()
    {
        _stats = GetComponent<IStats>();
        if (_stats != null)
        {
            _originalMoveSpeed = _stats.MoveSpeed;
        }
    }

    // 오브젝트 풀에서 재사용될 때, 모든 효과를 초기화
    void OnEnable()
    {
        // 모든 실행 중인 코루틴을 멈춤
        StopAllCoroutines();
        _activeCoroutines.Clear();

        // 속도를 원래대로 복구
        if (_stats != null)
        {
            _stats.MoveSpeed = _originalMoveSpeed;
        }
    }

    // 외부에서 호출할 '효과 적용' 함수들

    // 둔화 효과를 적용하는 함수
    public void ApplySlow(float amount, float duration)
    {
        if (_stats == null) return;

        // 이미 같은 종류(SlowEffect)의 효과가 걸려있다면, 이전 효과를 멈추고 새로 시작 (효과 갱신)
        StopEffect(typeof(SlowEffect));
        _activeCoroutines[typeof(SlowEffect)] = StartCoroutine(SlowCoroutine(amount, duration));
    }

    // 화상 효과를 적용하는 함수
    public void ApplyBurn(float damagePerTick, float tickInterval, float duration)
    {
        // 이전 화상 효과를 멈추고 새로운 코루틴 시작
        StopEffect(typeof(BurnEffect));
        _activeCoroutines[typeof(BurnEffect)] = StartCoroutine(BurnCoroutine(damagePerTick, tickInterval, duration));
    }


    // 실제 효과를 처리하는 코루틴들

    private IEnumerator SlowCoroutine(float amount, float duration)
    {
        // 1. 실제 능력치 변경
        _stats.MoveSpeed = _originalMoveSpeed * (1f - amount);

        // 2. 지속 시간만큼 대기
        yield return new WaitForSeconds(duration);

        // 3. 능력치 원상 복구
        _stats.MoveSpeed = _originalMoveSpeed;

        // 4. 관리 목록에서 제거
        _activeCoroutines.Remove(typeof(SlowEffect));
    }

    // --- 실제 화상 효과를 처리하는 코루틴 ---
    private IEnumerator BurnCoroutine(float damagePerTick, float tickInterval, float duration)
    {
        Debug.Log($"<color=red>화상 효과 적용!</color> {tickInterval}초마다 {damagePerTick}의 데미지를 입습니다. ({duration}초 지속)");

        float timer = duration;
        while (timer > 0)
        {
            Debug.Log($"<color=red>화상 틱 데미지! {-damagePerTick}</color>");

            // 다음 틱까지 대기
            yield return new WaitForSeconds(tickInterval);
            timer -= tickInterval;
        }

        _activeCoroutines.Remove(typeof(BurnEffect));
        Debug.Log("<color=red>화상 효과가 해제되었습니다.</color>");
    }

    // 코루틴을 안전하게 멈추기 위한 헬퍼 함수
    private void StopEffect(System.Type effectType)
    {
        if (_activeCoroutines.TryGetValue(effectType, out Coroutine coroutine))
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            _activeCoroutines.Remove(effectType);
        }
    }
}