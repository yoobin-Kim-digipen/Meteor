using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [Tooltip("이 시간이 지나면 오브젝트가 자동으로 파괴됩니다.")]
    public float lifetime = 2f;

    void Start()
    {
        // lifetime 초 후에 이 스크립트가 붙어있는 게임 오브젝트를 파괴
        Destroy(gameObject, lifetime);
    }
}