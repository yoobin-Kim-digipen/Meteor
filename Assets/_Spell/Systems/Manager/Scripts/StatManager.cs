using System.Collections.Generic;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }
    public GameObject playerObject;
    private Player player;
    public WeaponData weaponData;
    public bool isPlayerDead { get; private set; } = false;
    public float criticalHitChance = 0.3f; // 치명타 확률 (예: 10%)
    public float criticalHitMultiplier = 0.5f; // 치명타 피해 배율 (예: 50% 증가)
    private int intelligence = 20; // 플레이어의 지능
    private int defense = 20; // 플레이어 방어력
    private int experiencePoints = 0;
    private int currentLevel = 1;
    private float previousHealth = -1f;
    //[SerializeField] private int baseXPForNextLevel = 100;
    //private int XPRequired => currentLevel * baseXPForNextLevel;

    // 예시: 레벨 1~9까지
    private int[] requiredXPTable = { 100, 200, 300, 450, 650, 900, 1200, 1600 }; // 1단계~8단계 누적 경험치


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        player = playerObject.GetComponent<Player>();
        if (player == null)
            Debug.LogError("StatManager: Player script not found on playerObject.");
    }

    void Update()
    {
        MornitoringHP();
        //MornitoringMP();
    }

    public void MornitoringHP()
    {
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float health = playerHealth.currentHealth;
            if (health != previousHealth)
            {
                Debug.Log("StatManager가 확인한 플레이어 체력: " + health);
                previousHealth = health;
            }
            if (health <= 0 && !isPlayerDead)
            {
                isPlayerDead = true;
                Debug.LogWarning("플레이어가 사망하였습니다.");
                Time.timeScale = 0f;
                // 사망 시 추가 로직
            }
        }
    }

    public void MornitoringMP()
    {
        // MP 모니터링 로직 추가 예정
    }

    // 플레이어의 지능 증가
    public void GainINT(int intAmount)
    {
        intelligence += intAmount;
        Debug.Log("플레이어의 지능이 증가했습니다. 현재 지능: " + intelligence);
        List<SkillData> skilllist = weaponData.skills;
        foreach (var skill in skilllist)
        {
            if (skill is ProjectileSkillData projSkill)
            {
                float originalDamage = projSkill.damage;
                float increasedDamage = originalDamage * (1 + intelligence * 0.05f);
                projSkill.damage = increasedDamage;
                Debug.Log($"스킬 {projSkill.skillName}의 데미지가 {originalDamage}에서 {increasedDamage}로 증가했습니다.");
            }
            // 다른 스킬 타입도 처리 추가 가능
        }
        weaponData.skills = skilllist;
    }

    public void GainDEF(int amount)
    {
        defense += amount;
        Debug.Log("플레이어의 방어력이 증가했습니다. 현재 방어력: " + defense);
    }

    public void AdjustingSPD(int amount)
    {
        player.moveSpeed += amount;
        Debug.Log("플레이어의 이동 속도가 증가했습니다. 현재 이동 속도: " + player.moveSpeed);
    }

    public void GainExperience(int amount)
    {
        experiencePoints += amount;
        Debug.Log("플레이어가 " + amount + " 경험치를 획득했습니다. 현재 경험치: " + experiencePoints);
        // 이후 레벨업 과정 수정 예정
    }

    public int GetXP()
    {
        return experiencePoints;
    }

    public int GetRequiredXP()
    {
        // currentLevel은 1부터 시작, 인덱스는 0부터 시작하므로 -1
        if (currentLevel - 1 < requiredXPTable.Length)
            return requiredXPTable[currentLevel - 1];
        else
            return requiredXPTable[requiredXPTable.Length - 1]; // 최대값 고정
    }

    public void LevelUp()
    {
        int xpNeeded = GetRequiredXP();
        experiencePoints -= xpNeeded;
        currentLevel++;
        Debug.Log($"<color=red>Level Up! 현재 레벨: {currentLevel}</color>, 다음 레벨 필요 XP: {GetRequiredXP()}");
        Object.FindAnyObjectByType<PopupManager>()?.ShowPopup("Level Up!");
        // 추가 성장 로직
    }

    // ===== 핵심 데미지 계산 메서드 =====
    private const int DamageDefenseConstant = 100; // K 값, 방어 상수

    /// <summary>
    /// 최종 데미지 계산 (스킬 위력/지능/방어/치명타)
    public int CalculateFinalDamage(int baseSkillDamage, int monsterMDEF)
    {
        // 변수 정의
        int INT = intelligence;
        float CR = criticalHitChance;
        float CD = criticalHitMultiplier;

        // Q = P × (1 + 0.1 × INT)
        float Q = baseSkillDamage * (1 + 0.1f * INT);

        // D = Q × (K / (MDEF + K))
        float D = Q * (DamageDefenseConstant / (float)(monsterMDEF + DamageDefenseConstant));

        // 치명타 판정
        bool isCritical = UnityEngine.Random.value < CR;
        float F;
        if (isCritical)
        {
            F = D * (1 + CD); // CD ex: 0.5 = 50%
            Debug.Log("<color=yellow>치명타! 데미지: " + Mathf.FloorToInt(F) + "</color>");
        }
        else
        {
            F = D;
            Debug.Log("일반 공격 데미지: " + Mathf.FloorToInt(F));
        }
        return Mathf.FloorToInt(F); // 깔끔하게 소수점 절삭
    }
}
