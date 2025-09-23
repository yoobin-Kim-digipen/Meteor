using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform target;                 // ���� ���(�÷��̾�)
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f); // �Ӹ� ����

    public float distance = 15f;             // �⺻ �Ÿ�
    public float minDistance = 5f;           // �ּ�
    public float maxDistance = 20f;          // �ִ�

    public float yaw = 0f;                   // �¿� ����
    public float pitch = 15f;                // ���Ʒ� ����
    public float minPitch = -30f;            // �Ʒ��� �ִ�
    public float maxPitch = 70f;             // ���� �ִ�
    public float sensitivity = 0.12f;        // ���콺 �ΰ���
    public bool invertY = false;             // ���콺 Y ����

    public float zoomStep = 25f;             // �� �� ĭ �� �Ÿ� ��ȭ

    // �ʱⰪ ���(�� Ŭ�� ���¿�)
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
        // target ������� Player �±� �ڵ� �Ҵ�
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

        float scroll = Mouse.current.scroll.ReadValue().y; // ���� ��120
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float steps = scroll / 120f; // �� ĭ ����
            distance = Mathf.Clamp(distance - steps * zoomStep, minDistance, maxDistance);
        }
    }
}
