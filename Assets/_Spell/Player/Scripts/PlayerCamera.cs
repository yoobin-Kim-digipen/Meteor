using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform target;                 // 따라갈 대상(플레이어)
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f); // 머리 높이

    public float distance = 15f;             // 기본 거리
    public float minDistance = 5f;           // 최소
    public float maxDistance = 20f;          // 최대

    public float yaw = 0f;                   // 좌우 각도
    public float pitch = 15f;                // 위아래 각도
    public float minPitch = -30f;            // 아래로 최대
    public float maxPitch = 70f;             // 위로 최대
    public float sensitivity = 0.12f;        // 마우스 민감도
    public bool invertY = false;             // 마우스 Y 반전

    public float zoomStep = 25f;             // 휠 한 칸 당 거리 변화

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

        CheckResetInput();
        Camera_Movement();
        Camera_Zoomsetup();

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + pivotOffset;
        Vector3 pos = focus - rot * Vector3.forward * distance;

        transform.position = pos;
        transform.LookAt(focus);
    }

    void CheckResetInput()
    {
        if (Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame)
        {
            pitch = defaultPitch;
            distance = defaultDistance;
            pivotOffset = defaultPivotOffset;
        }
    }

    void Camera_Movement()
    {
        if (Mouse.current == null) return;

        Vector2 m = Mouse.current.delta.ReadValue();
        float mx = m.x * sensitivity;
        float my = m.y * sensitivity * (invertY ? 1f : -1f);

        yaw += mx;
        pitch = Mathf.Clamp(pitch + my, minPitch, maxPitch);
    }

    void Camera_Zoomsetup()
    {
        if (Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y; // 보통 ±120
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float steps = scroll / 120f; // 한 칸 단위
            distance = Mathf.Clamp(distance - steps * zoomStep, minDistance, maxDistance);
        }
    }
}
