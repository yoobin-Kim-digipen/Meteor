using System.Collections.Generic;
using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }
    public GameObject playerObject; // Reference to the Player script
    private Player player;
    public bool isPlayerDead { get; private set; } = false;
    private int intelligence = 0; // 플레이어의 지능 수치
    private int defense = 0; // 플레이어의 방어력 수치
    public float criticalHitChance = 0.1f; // 치명타 확률 (예: 10%)
    public float criticalHitMultiplier = 1.5f; // 치명타 피해 배율 (예: 1.5배)
    private int experiencePoints = 0; // 플레이어의 경험치
    private int currentLevel = 1; // 플레이어의 현재 레벨
    [SerializeField] private int baseXPForNextLevel = 100; // 1레벨에서 2레벨까지의 기본 XP (100)


    // 다음 레벨에 필요한 총 경험치
    private int XPRequired => currentLevel * baseXPForNextLevel;

    private void Awake()
    {
        // 싱글톤 인스턴스 할당 및 중복 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        player = playerObject.GetComponent<Player>();
        if (player == null)
        {
            Debug.LogError("StatManager: Player script not found on the assigned playerObject.");
        }
    }

    void Update()
    {
        //MornitoringHP();
        MornitoringMP();
    }

    public void MornitoringHP()
    {
        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float health = playerHealth.currentHealth;
            Debug.Log("StatManager가 확인한 플레이어 체력: " + health);
            if (health <= 0 && !isPlayerDead)
            {
                isPlayerDead = true;
                Debug.LogWarning("플레이어가 사망하였습니다.");
                // 사망 시 추가 로직
            }
        }
    }

    public void MornitoringMP()
    {
        // MP 모니터링 로직 추가 예정
    }

    // 버그 생겼음. 주석처리. 회의 후 수정 예정
    // 지금 발사체 데미지만 있는데 스킬 자체 데미지가 없어서 문제가 생김
    // 플레이어의 모든 공격의 위력을 증가시키려면 스킬 자체 데미지도 존재해야 함
    // public void GainINT(int intAmount)
    // {
    //     intelligence += intAmount;
    //     Debug.Log("플레이어의 지능이 증가했습니다. 현재 지능: " + intelligence);
    //     PlayerAttackManager playerAttackManager = playerObject.GetComponent<PlayerAttackManager>();
    //     if (playerAttackManager != null)
    //     {
    //         List<WeaponData> weaponlist = playerAttackManager.equippedWeapons;
    //         foreach (var weapon in weaponlist)
    //         {
    //             weapon.damage += 10; // 예시: 모든 무기의 데미지를 10 증가
    //         }
    //     }
    // }

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
    }

    public int GetXP()
    {
        return experiencePoints;
    }

    public void LevelUp()
    {
        int xpNeeded = XPRequired;
        experiencePoints -= xpNeeded;
        currentLevel++;
        Debug.Log($"Level Up! 현재 레벨: {currentLevel}, 다음 레벨 필요 XP: {XPRequired}");
    }
}
