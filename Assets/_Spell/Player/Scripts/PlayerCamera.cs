using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform target;                 // ���� ���(�÷��̾�)
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f); // �Ӹ� ����

    public float distance = 5f;             // �⺻ �Ÿ�
    public float minDistance = 5f;           // �ּ�
    public float maxDistance = 20f;          // �ִ�
    public float maxHeightOffset = 5f;       // �ִ� �� �ƿ� �� �߰��� ����

    public float yaw = 0f;                   // �¿� ����
    public float pitch = 15f;                // ���Ʒ� ����
    public float minPitch = -85f;            // �Ʒ��� �ִ�
    public float maxPitch = 85f;             // ���� �ִ�
    public float sensitivity = 0.12f;        // ���콺 �ΰ���
    public bool invertY = false;             // ���콺 Y ����

    public float zoomStep = 25f;             // �� �� ĭ �� �Ÿ� ��ȭ

    [Header("Collision")]
    public LayerMask collisionMask;      // ī�޶� �浹�� ���̾�� (��: Ground, Wall)
    public float collisionPadding = 0.2f; // �浹 �������� ��¦ �� �Ÿ� (ī�޶� ���� �ʹ� ���� �ʰ�)

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

        checkResetInput();
        camera_Movement();
        camera_Zoomsetup();

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + pivotOffset;
        Vector3 pos = focus - rot * Vector3.forward * distance;

        Vector3 finalPos = camera_Raycast(focus, pos); //���� �ٴڶ� ������

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

        // 1. ���� �� ����
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float steps = scroll / 15f;
            distance = Mathf.Clamp(distance - steps * zoomStep, minDistance, maxDistance);
        }

        // 2. �߰��� ����: �Ÿ��� ����Ͽ� pivotOffset�� y���� ����
        // ���� �� ���°� �ּ�(0)���� �ִ�(1)���� ������ ���
        float zoomRatio = (distance - minDistance) / (maxDistance - minDistance);

        // �⺻ ����(defaultPivotOffset.y)���� �߰��� �ִ� ����(maxHeightOffset) ���̸� ����
        float newOffsetY = Mathf.Lerp(defaultPivotOffset.y, defaultPivotOffset.y + maxHeightOffset, zoomRatio);

        // pivotOffset�� y���� ������Ʈ
        pivotOffset.y = newOffsetY;
    }

    Vector3 camera_Raycast(Vector3 focus, Vector3 pos)
    {
        RaycastHit hit;

        // �÷��̾� �Ӹ�(focus)���� �̻����� ī�޶� ��ġ(pos) �������� Ray�� ���.
        // Vector3.forward ��� (pos - focus).normalized �� ����� ��Ȯ�� ������ ���Ѵ�.
        Vector3 direction = pos - focus;
        float rayDistance = direction.magnitude; // ������ �ִ� �Ÿ��� ���� distance�� ����.
        Vector3 finalPosition = pos; // ���� ��ġ�� �ϴ� ���Ƿ� �ʱ�ȭ

        // Raycast ����
        if (Physics.Raycast(focus, direction.normalized, out hit, rayDistance, collisionMask))
        {
            // ���� Ray�� ���𰡿� �ε����ٸ�,
            // ���� ��ġ�� '�ε��� ����'���� '�е�'��ŭ ��¦ ������ ��� ������ �����Ѵ�.
            Debug.DrawLine(focus, hit.point, Color.red); // ������: �浹 �������� ���� �� �׸���
            return hit.point + hit.normal * collisionPadding;
        }
        else
        {
            Debug.DrawLine(focus, pos, Color.green); // ������: �浹 ���� �� �ʷ� �� �׸���
            return pos;
        }
    }
}
