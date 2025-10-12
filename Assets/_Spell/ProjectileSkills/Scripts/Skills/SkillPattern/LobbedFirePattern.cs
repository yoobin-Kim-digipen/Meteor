using UnityEngine;

// 새로운 발사 패턴: 포물선
public class LobbedFirePattern : IFirePattern
{
    public void Execute(GameObject caster, SkillData data, Vector3 spawnPos, Quaternion baseRotation, Vector3 targetPoint)
    {
        if (data is LobbedSkillData lobbedData)
        {
            Vector3? launchVelocity = CalculateLaunchVelocity(spawnPos, targetPoint, lobbedData.launchAngle);

            if (launchVelocity.HasValue)
            {
                GameObject skillObj = ObjectPooler.Instance.GetFromPool(lobbedData.skillName, spawnPos, Quaternion.identity);
                if (skillObj != null)
                {
                    if (skillObj.TryGetComponent<Rigidbody>(out var rb))
                    {
                        rb.linearVelocity = launchVelocity.Value;
                    }
                    skillObj.GetComponent<Skill>()?.Activate(caster, lobbedData);
                }
            }
        }
    }

    // 포물선 속도 계산 함수
    private Vector3? CalculateLaunchVelocity(Vector3 startPoint, Vector3 endPoint, float launchAngle)
    {
        float gravity = Physics.gravity.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0, endPoint.z - startPoint.z);
        float distanceXZ = displacementXZ.magnitude;
        float heightY = endPoint.y - startPoint.y;

        float angleInRadians = launchAngle * Mathf.Deg2Rad;
        float cosAngle = Mathf.Cos(angleInRadians);
        float sinAngle = Mathf.Sin(angleInRadians);
        float tanAngle = Mathf.Tan(angleInRadians);

        // 분모가 0에 가까워지거나 음수가 되면 발사 불가 (수학적 오류)
        float denominator = 2 * (heightY - distanceXZ * tanAngle) * cosAngle * cosAngle;
        if (Mathf.Approximately(denominator, 0)) return null;

        float speedSquared = (gravity * distanceXZ * distanceXZ) / denominator;
        if (speedSquared < 0) return null;

        float launchSpeed = Mathf.Sqrt(speedSquared);

        return displacementXZ.normalized * launchSpeed * cosAngle + Vector3.up * launchSpeed * sinAngle;
    }
}