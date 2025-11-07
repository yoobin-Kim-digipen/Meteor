using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ParrySkill : BasicSkill
{
    private ParrySkillData _data;
    private GameObject _caster;
    private bool _isParrying = false;
    private GameObject _shieldInstance;
    public bool WasParrySuccessful { get; private set; }

    private Camera _mainCamera;

    [Header("Component Settings")]
    [Tooltip("플레이어를 기준으로 방패가 생성될 상대적 위치를 조절합니다. Y: 높이, Z: 거리")]
    [SerializeField] private Vector3 shieldSpawnOffset = new Vector3(0, 1.0f, 0.8f);
    [Tooltip("생성될 방패의 기본 회전 값을 보정합니다.")]
    [SerializeField] private Vector3 shieldRotationOffset = new Vector3(-90f, 0, 0);

    private void Awake()
    {
        // 성능을 위해 메인 카메라를 미리 찾아 저장해 둡니다.
        _mainCamera = Camera.main;
    }

    public override void Activate(GameObject caster, BasicSkillData data)
    {
        if (_isParrying) return;

        _caster = caster;
        if (data is ParrySkillData parryData)
        {
            _data = parryData;
            StartCoroutine(ParryWindowCoroutine());
        }
    }

    private IEnumerator ParryWindowCoroutine()
    {
        _isParrying = true;
        WasParrySuccessful = false;

        if (_data.shieldPrefab != null && _mainCamera != null)
        {
            // ▼▼▼▼▼ 카메라 기준 방패 생성 로직으로 변경되었습니다 ▼▼▼▼▼
            
            // 1. 카메라가 바라보는 수평 방향을 계산합니다.
            Vector3 camForward = _mainCamera.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            // 2. 플레이어 위치를 기준으로, 카메라 방향으로 떨어진 위치를 계산합니다.
            //    Z offset은 앞으로의 거리, Y offset은 플레이어 발밑 기준 높이가 됩니다.
            Vector3 shieldPosition = _caster.transform.position + (camForward * shieldSpawnOffset.z);
            shieldPosition.y = _caster.transform.position.y + shieldSpawnOffset.y;

            // 3. 방패가 카메라와 같은 방향을 바라보도록 회전 값을 계산합니다.
            Quaternion shieldRotation = Quaternion.LookRotation(camForward) * Quaternion.Euler(shieldRotationOffset);

            // 4. 계산된 위치와 회전으로 방패를 생성합니다.
            _shieldInstance = Instantiate(_data.shieldPrefab, shieldPosition, shieldRotation);
        }

        float timer = 0f;
        while (timer < _data.parryWindowDuration && !WasParrySuccessful)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        
        if (_shieldInstance != null)
        {
            Destroy(_shieldInstance);
        }
        _isParrying = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isParrying || WasParrySuccessful || _data == null) return;

        if ((_data.parryableLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            WasParrySuccessful = true; 
            
            Debug.Log("<color=lime>Parry Success!</color>");
            
            other.gameObject.SetActive(false);
            
            if (_data.successVFX != null)
            {
                Instantiate(_data.successVFX, _caster.transform.position, _caster.transform.rotation);
            }
            
            StatManager.Instance.HandleSuccessfulParry(_data.slowdownFactor, _data.slowdownDuration);
        }
    }
}

