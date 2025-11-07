using UnityEngine;

[CreateAssetMenu(fileName = "New Teleport Skill", menuName = "Skills/Teleport Skill")]
public class TeleportSkillData : BasicSkillData // BaseSkillData를 상속
{
    [Header("Teleport Stats")]
    public float teleportDistance = 10f;
    [Tooltip("순간이동 시작 지점에 생성될 시각 효과")]
    public GameObject startVFX;
    [Tooltip("순간이동 도착 지점에 생성될 시각 효과")]
    public GameObject endVFX;
}