using UnityEngine;

public class PlayerSkillManager : MonoBehaviour
{
    public static PlayerSkillManager Instance { get; private set; }

    [Header("Player-Centric Skill Handlers")]
    [Tooltip("텔레포트 스킬의 실제 로직을 처리하는 핸들러")]
    public TeleportSkill teleportSkillHandler;
    public ParrySkill parrySkillHandler;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("PlayerSkillManager의 중복 인스턴스가 생성되어 파괴합니다.");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // 씬이 바뀌어도 파괴되지 않게 하려면 아래 코드의 주석을 해제
            // DontDestroyOnLoad(gameObject);
        }
    }

    /// 외부에서 스킬 사용을 요청할 때 호출하는 함수
    public void UseSkill(BasicSkillData skillData, GameObject caster)
    {
        if (skillData == null || caster == null)
        {
            Debug.LogError("스킬 데이터 또는 시전자가 없습니다.");
            return;
        }

        // 스킬 데이터의 종류를 확인하여 그에 맞는 핸들러를 호출
        if (skillData is TeleportSkillData teleportData && teleportSkillHandler != null)
        {
            teleportSkillHandler.Activate(caster, teleportData);
        }
        else if (skillData is ParrySkillData parryData && parrySkillHandler != null)
        {
            parrySkillHandler.Activate(caster, parryData);
        }
        else
        {
            Debug.LogWarning($"'{skillData.skillName}' 스킬을 처리할 수 있는 핸들러가 PlayerSkillManager에 없거나, 데이터 타입이 잘못되었습니다.");
        }
    }
}