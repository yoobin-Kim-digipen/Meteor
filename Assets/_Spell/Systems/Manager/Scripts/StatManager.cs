using System.Collections.Generic;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }
    public GameObject playerObject;
    private Player player;
    public WeaponData weaponData;
    public bool isPlayerDead { get; private set; } = false;
    private float criticalHitChance = 0.3f; // 치명타 확률 (예: 10%)
    private float criticalHitMultiplier = 0.5f; // 치명타 피해 배율 (예: 50% 증가)
    private int intelligence = 20; // 플레이어의 지능
    private int defense = 20; // 플레이어 방어력
    private int experiencePoints = 0;
    private int currentLevel = 1;
    private float previousHealth = -1f;
    public PlayerHealth playerHealth { get; private set; }
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

        playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            Debug.LogError("StatManager: PlayerHealth script not found on playerObject.");

    }

    void Update()
    {
        MornitoringHP();
        //MornitoringMP();
    }

    public void AdjustingHP(int amount)
    {
        playerHealth.maxHealth += amount;
        Debug.Log("플레이어의 최대 체력이 " + amount + "만큼 조정되었습니다. 현재 최대 체력: " + playerHealth.maxHealth);
    }

    public void MornitoringHP()
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

    public void MornitoringMP()
    {
        // MP 모니터링 로직 추가 예정
    }

    private float damageMultiplier = 1f;  // 지능 증가에 따른 데미지 배율
    // 플레이어의 지능 증가
    public void GainINT(int intAmount)
    {
        intelligence += intAmount;
        damageMultiplier = 1f + intelligence * 0.05f;  // 지능에 따라 데미지 배율 업데이트

        Debug.Log("플레이어의 지능이 증가했습니다. 현재 지능: " + intelligence);

        // 스킬별 원본 데미지는 그대로 유지, 로그 출력용만 데미지 계산해서 보여줌
        foreach (var skill in weaponData.skills)
        {
            if (skill is ProjectileSkillData projSkill)
            {
                float originalDamage = projSkill.damage;
                float increasedDamage = originalDamage * damageMultiplier;
                Debug.Log($"스킬 {projSkill.skillName}의 데미지가 {originalDamage}에서 {increasedDamage}로 증가했습니다.");
            }
            // 다른 스킬 타입도 비슷하게 처리 가능
        }
    }


    public void GainDEF(int amount)
    {
        defense += amount;
        Debug.Log("플레이어의 방어력이 증가했습니다. 현재 방어력: " + defense);
    }

    private int spd = 20;

    public float CalculateMoveSpeed()
    {
        float calculated = player.moveSpeed * (1 + 0.01f * spd);
        return Mathf.Max(player.moveSpeed, calculated);
    }

    public void AdjustingSPD(int amount)
    {
        spd += amount;
        player.moveSpeed = CalculateMoveSpeed();
        Debug.Log($"SPD: {spd}, 적용된 이동속도: {player.moveSpeed}");
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

    private int lastChosenRouteIdx = -1;
    private int[] mainCoreLevels = new int[3]; // 0: 망령화, 1: 분노, 2: 냉철함
    private int[] subCoreLevels = new int[3]; // 0: 망령화, 1: 분노, 2: 냉철함
    private bool hasChosenRoute = false;

    // 단순 레벨업 처리 (경험치 감소 및 레벨 증가에만 집중)
    public void LevelUp()
    {
        int xpNeeded = GetRequiredXP();
        experiencePoints -= xpNeeded;
        currentLevel++;
        Debug.Log($"<color=yellow>Level Up! 현재 레벨: {currentLevel}</color>");

        // 최초 레벨업 때만 성장 루트 선택 UI 표시
        if (!hasChosenRoute)
        {
            string[] options = { "Ghosting", "Anger", "Cool-headedness" };
            Object.FindAnyObjectByType<LevelUpChoiceUI>()?.ShowLevelUpChoices(options, OnChooseGrowthOption);
        }
        else
        {
            // 이미 선택된 루트로 계속 성장 처리
            ApplyGrowthByChosenRoute();
        }
        playerHealth.currentHealth = playerHealth.maxHealth; // 레벨업 시 체력 회복
        previousHealth = playerHealth.currentHealth; // 체력 모니터링 초기화
    }

    // 성장 선택 콜백에서 실제 성장 처리
    private void OnChooseGrowthOption(int selectedIdx)
    {
        lastChosenRouteIdx = selectedIdx;
        hasChosenRoute = true;  // 선택 완료 표시
        Debug.Log($"선택한 성장 옵션 인덱스: {lastChosenRouteIdx}");

        ApplyGrowthByChosenRoute();
    }

    private void ApplyGrowthByChosenRoute()
    {
        if (currentLevel == 2 || currentLevel == 4 || currentLevel == 6 || currentLevel == 8 || currentLevel ==  9)
        {
            ApplyMainCoreUpgrade(lastChosenRouteIdx);
            ApplyMainCoreEffect(lastChosenRouteIdx, mainCoreLevels[lastChosenRouteIdx]);
        }

        if (currentLevel == 3 || currentLevel == 5 || currentLevel == 7)
        {
            ApplySubCoreUpgrade(lastChosenRouteIdx);
            ApplySubCoreEffect(lastChosenRouteIdx, subCoreLevels[lastChosenRouteIdx]);
        }
    }


    private static readonly string[] RouteNames = { "망령화", "분노", "냉철함" };

    private void ApplyMainCoreUpgrade(int routeIdx)
    {
        if (routeIdx >= 0 && routeIdx < mainCoreLevels.Length)
        {
            mainCoreLevels[routeIdx]++;
            Debug.Log($"<color=yellow>{RouteNames[routeIdx]} 메인코어 {mainCoreLevels[routeIdx]}단계 달성.</color>");
        }
    }

    private void ApplySubCoreUpgrade(int routeIdx)
    {
        if (routeIdx >= 0 && routeIdx < subCoreLevels.Length)
        {
            subCoreLevels[routeIdx]++;
            Debug.Log($"<color=yellow>{RouteNames[routeIdx]} 서브코어 {subCoreLevels[routeIdx]}단계 달성.</color>");
        }
    }

    // ===== 메인코어 스탯스킬트리 =====
    private void ApplyMainCoreEffect(int routeIdx, int level)
    {
        // 각 루트/단계별 효과 부여
        if (routeIdx == 0) // 망령화 루트
        {
            switch (level)
            {
                case 1:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 2:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 3:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 4:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 5:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
            }
        }
        if (routeIdx == 1) // 분노 루트 
        {
            switch (level)
            {
                case 1:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 2:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 3:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 4:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 5:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
            }
        }
        if (routeIdx == 2) // 냉철함 루트
        {
            switch (level)
            {
                case 1:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 2:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 3:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 4:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
                case 5:
                    // 추가 스탯 또는 스킬 변경 요소 추가 예정
                    break;
            }
        }
    }

    // ===== 서브코어 스탯트리 ===== (기획 바탕으로 임시 완성)
    private void ApplySubCoreEffect(int routeIdx, int level)
    {
        // 각 루트/단계별 효과 부여
        if (routeIdx == 0) // 망령화 루트
        {
            switch (level)
            {
                case 1:
                    AdjustingHP(100);
                    GainINT(10);
                    GainDEF(10);
                    AdjustingSPD(5);
                    break;
                case 2:
                    AdjustingHP(150);
                    GainINT(15);
                    GainDEF(15);
                    AdjustingSPD(10);
                    break;
                case 3:
                    AdjustingHP((int)(experiencePoints * 0.05f));
                    GainINT((int)(intelligence * 0.05f));
                    GainDEF((int)(defense * 0.05f));
                    AdjustingSPD((int)(player.moveSpeed * 0.07f));
                    break;
            }
        }
        if (routeIdx == 1) // 분노 루트
        {
            switch (level)
            {
                case 1:
                    AdjustingHP(100);
                    GainINT(10);
                    GainDEF(10);
                    AdjustingSPD(5);
                    break;
                case 2:
                    AdjustingHP(150);
                    GainINT(15);
                    GainDEF(15);
                    AdjustingSPD(10);
                    break;
                case 3:
                    AdjustingHP((int)(experiencePoints * 0.05f));
                    GainINT((int)(intelligence * 0.07f));
                    GainDEF((int)(defense * 0.05f));
                    AdjustingSPD((int)(player.moveSpeed * 0.05f));
                    break;
            }
        }
        if (routeIdx == 2) // 냉철함 루트
        {
            switch (level)
            {
                case 1:
                    AdjustingHP(100);
                    GainINT(10);
                    GainDEF(10);
                    AdjustingSPD(5);
                    break;
                case 2:
                    AdjustingHP(150);
                    GainINT(15);
                    GainDEF(15);
                    AdjustingSPD(10);
                    break;
                case 3:
                    AdjustingHP((int)(experiencePoints * 0.05f));
                    GainINT((int)(intelligence * 0.05f));
                    GainDEF((int)(defense * 0.07f));
                    AdjustingSPD((int)(player.moveSpeed * 0.05f));
                    break;
            }
        }
    }

    // ===== 핵심 데미지 계산 메서드 =====
    private const int DamageDefenseConstant = 100; // K 값, 방어 상수

    /// <summary>
    /// 최종 데미지 계산 (스킬 위력/지능/방어/치명타)
    public int CalculateFinalDamage(int baseSkillDamage, int monsterMDEF)
    {
        int INT = intelligence;
        float CR = criticalHitChance;
        float CD = criticalHitMultiplier;

        float effectiveDamage = baseSkillDamage * damageMultiplier;

        float Q = effectiveDamage * (1 + 0.1f * INT);
        float D = Q * (DamageDefenseConstant / (float)(monsterMDEF + DamageDefenseConstant));

        bool isCritical = UnityEngine.Random.value < CR;
        float F;

        if (isCritical)
        {
            F = D * (1 + CD);
            Debug.Log("<color=yellow>치명타! 데미지: " + Mathf.FloorToInt(F) + "</color>");
        }
        else
        {
            F = D;
            Debug.Log("일반 공격 데미지: " + Mathf.FloorToInt(F));
        }
        return Mathf.FloorToInt(F);
    }

}
