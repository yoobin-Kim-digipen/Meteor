using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 반드시 필요

public class HealthBarUI : MonoBehaviour
{
    // Slider 컴포넌트 자체를 연결할 변수
    private Slider healthSlider;

    // 💡 StatManager와 PlayerHealth 컴포넌트를 연결할 변수
    // 이들을 통해 체력 데이터를 가져옵니다.
    public StatManager statManagerRef;
    public PlayerHealth playerHealthRef;

    void Awake()
    {
        // 1. Slider 컴포넌트 가져오기 (이 스크립트가 Slider에 부착되어 있음)
        healthSlider = GetComponent<Slider>();
        
        // StatManager를 통해 PlayerHealth 컴포넌트를 가져옵니다.
        if (playerHealthRef == null && statManagerRef != null && statManagerRef.playerObject != null)
        {
            playerHealthRef = statManagerRef.playerObject.GetComponent<PlayerHealth>();
        }

        // 2. 최대 체력 설정 (플레이어의 maxHealth를 가져옴)
        if (playerHealthRef != null)
        {
            healthSlider.maxValue = playerHealthRef.maxHealth;
            // 초기 체력으로 Value 설정
            healthSlider.value = playerHealthRef.currentHealth;
        }
    }

    void Update()
    {
        // 3. 매 프레임 플레이어의 현재 체력으로 Slider 값을 업데이트
        if (playerHealthRef != null)
        {
            healthSlider.value = playerHealthRef.currentHealth;
        }
    }
}