using UnityEngine;

[CreateAssetMenu(fileName = "New Parry Skill", menuName = "Skills/Parry Skill")]
public class ParrySkillData : BasicSkillData
{
    [Header("Parry Settings")]
    [Tooltip("패링이 가능한 시간")]
    public float parryWindowDuration = 0.3f;

    [Tooltip("패링 시 나타날 방패 프리팹")]
    public GameObject shieldPrefab;

    [Tooltip("패링할 수 있는 대상의 레이어를 선택합니다.")]
    public LayerMask parryableLayers; // LayerMask 변수 추가

    [Header("Success Effects")]
    [Tooltip("패링 성공 시 게임 시간 감속 비율")]
    public float slowdownFactor = 0.5f;

    [Tooltip("시간 감속이 지속되는 시간")]
    public float slowdownDuration = 3f;

    [Tooltip("패링 성공 시 나타날 시각 효과(VFX) 프리팹")]
    public GameObject successVFX;
}
