using UnityEngine;
using System.Collections; // Coroutine을 위해 추가

public class TeleportSkill : BasicSkill
{
    private TeleportSkillData _data;
    private GameObject _caster;
    public bool IsTeleporting { get; private set; } = false;

    public override void Activate(GameObject caster, BasicSkillData data)
    {
        // 이미 텔레포트 중이라면 중복 실행 방지
        if (IsTeleporting)
        {
            return;
        }

        _caster = caster;
        if (data is TeleportSkillData teleportData)
        {
            _data = teleportData;
            // Coroutine으로 텔레포트 실행
            StartCoroutine(ExecuteTeleportCoroutine());
        }
        else
        {
            Debug.LogError("잘못된 스킬 데이터가 TeleportSkill에 전달되었습니다.");
        }
    }

    private IEnumerator ExecuteTeleportCoroutine()
    {
        // 1. 텔레포트 상태 시작
        IsTeleporting = true;

        Transform casterTransform = _caster.transform;
        Vector3 startPosition = casterTransform.position;

        // 수평 방향 계산 (땅으로 파고드는 문제 해결)
        Vector3 forwardDirection = casterTransform.forward;
        forwardDirection.y = 0;
        forwardDirection.Normalize();

        if (forwardDirection.sqrMagnitude < 0.01f)
        {
            forwardDirection = casterTransform.root.forward;
            forwardDirection.y = 0;
            forwardDirection.Normalize();
        }

        Vector3 desiredEndPosition = startPosition + forwardDirection * _data.teleportDistance;

        // 벽과 같은 수직 장애물 확인
        Vector3 positionAfterWallCheck = CheckForObstacles(startPosition, desiredEndPosition);

        // 도착 지점의 바닥 높이에 맞춰 최종 위치 보정
        Vector3 finalPosition = AdjustHeightToGround(positionAfterWallCheck);

        Debug.Log("텔레포트 중...");

        // VFX 생성 및 위치 이동
        if (_data.startVFX != null)
        {
            Instantiate(_data.startVFX, startPosition, casterTransform.rotation);
        }

        // 짧은 딜레이 후 위치 이동 (VFX가 보일 시간 확보)
        yield return new WaitForSeconds(0.1f);

        casterTransform.position = finalPosition;

        if (_data.endVFX != null)
        {
            Instantiate(_data.endVFX, finalPosition, casterTransform.rotation);
        }

        // 스킬이 완전히 끝났음을 알리기 위한 짧은 추가 딜레이
        yield return new WaitForSeconds(0.2f);

        // 2. 텔레포트 상태 종료
        IsTeleporting = false;
    }

    private Vector3 CheckForObstacles(Vector3 start, Vector3 end)
    {
        Vector3 rayStart = start + Vector3.up * 0.5f;
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        RaycastHit hit;
        if (Physics.Raycast(rayStart, direction, out hit, distance))
        {
            Debug.Log($"텔레포트 경로에 장애물({hit.collider.name})이 감지되었습니다.");
            return hit.point - direction * 0.5f;
        }

        return end;
    }
    
    private Vector3 AdjustHeightToGround(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(position.x, position.y + 2f, position.z), Vector3.down, out hit, 4f))
        {
            return new Vector3(position.x, hit.point.y + 1f, position.z);
        }
        
        return position;
    }
}

