using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform target;                 // 따라갈 대상(플레이어)
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f); // 머리 높이

    public float distance = 5f;             // 기본 거리
    public float minDistance = 5f;           // 최소
    public float maxDistance = 20f;          // 최대
    public float maxHeightOffset = 5f;       // 최대 줌 아웃 시 추가될 높이

    public float yaw = 0f;                   // 좌우 각도
    public float pitch = 15f;                // 위아래 각도
    public float minPitch = -85f;            // 아래로 최대
    public float maxPitch = 85f;             // 위로 최대
    public float sensitivity = 0.12f;        // 마우스 민감도
    public bool invertY = false;             // 마우스 Y 반전

    public float zoomStep = 25f;             // 휠 한 칸 당 거리 변화

    [Header("Collision")]
    public LayerMask collisionMask;      // 카메라가 충돌할 레이어들 (예: Ground, Wall)
    public float collisionPadding = 0.2f; // 충돌 지점에서 살짝 뗄 거리 (카메라가 벽에 너무 붙지 않게)

    // 초기값 백업(휠 클릭 리셋용)
    private float defaultYaw, defaultPitch, defaultDistance;
    private Vector3 defaultPivotOffset;

    void Awake()
    {
        defaultYaw = yaw;
        defaultPitch = pitch;
        defaultDistance = distance;
        defaultPivotOffset = pivotOffset;
    }

    void Start()
    {
        // target 비었으면 Player 태그 자동 할당
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        checkResetInput();
        camera_Movement();
        camera_Zoomsetup();

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + pivotOffset;
        Vector3 pos = focus - rot * Vector3.forward * distance;

        Vector3 finalPos = camera_Raycast(focus, pos); //벽뚫 바닥뚫 방지용

        transform.position = finalPos;
        transform.LookAt(focus);
    }

    void checkResetInput()
    {
        if (Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame)
        {
            pitch = defaultPitch;
            distance = defaultDistance;
            pivotOffset = defaultPivotOffset;
        }
    }

    void camera_Movement()
    {
        if (Mouse.current == null) return;

        Vector2 m = Mouse.current.delta.ReadValue();
        float mx = m.x * sensitivity;
        float my = m.y * sensitivity * (invertY ? 1f : -1f);

        yaw += mx;
        pitch = Mathf.Clamp(pitch + my, minPitch, maxPitch);
    }

    void camera_Zoomsetup()
    {
        if (Mouse.current == null) return;

        // 1. 기존 줌 로직
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float steps = scroll / 15f;
            distance = Mathf.Clamp(distance - steps * zoomStep, minDistance, maxDistance);
        }

        // 2. 추가된 로직: 거리에 비례하여 pivotOffset의 y값을 조절
        // 현재 줌 상태가 최소(0)인지 최대(1)인지 비율을 계산
        float zoomRatio = (distance - minDistance) / (maxDistance - minDistance);

        // 기본 높이(defaultPivotOffset.y)에서 추가될 최대 높이(maxHeightOffset) 사이를 보간
        float newOffsetY = Mathf.Lerp(defaultPivotOffset.y, defaultPivotOffset.y + maxHeightOffset, zoomRatio);

        // pivotOffset의 y값만 업데이트
        pivotOffset.y = newOffsetY;
    }

    Vector3 camera_Raycast(Vector3 focus, Vector3 pos)
    {
        RaycastHit hit;

        // 플레이어 머리(focus)에서 이상적인 카메라 위치(pos) 방향으로 Ray를 쏜다.
        // Vector3.forward 대신 (pos - focus).normalized 를 사용해 정확한 방향을 구한다.
        Vector3 direction = pos - focus;
        float rayDistance = direction.magnitude; // 광선의 최대 거리는 원래 distance와 같다.
        Vector3 finalPosition = pos; // 최종 위치는 일단 임의로 초기화

        // Raycast 실행
        if (Physics.Raycast(focus, direction.normalized, out hit, rayDistance, collisionMask))
        {
            // 만약 Ray가 무언가에 부딪혔다면,
            // 최종 위치를 '부딪힌 지점'에서 '패딩'만큼 살짝 앞으로 당긴 곳으로 설정한다.
            Debug.DrawLine(focus, hit.point, Color.red); // 디버깅용: 충돌 지점까지 빨간 선 그리기
            return hit.point + hit.normal * collisionPadding;
        }
        else
        {
            Debug.DrawLine(focus, pos, Color.green); // 디버깅용: 충돌 없을 때 초록 선 그리기
            return pos;
        }
    }
}
