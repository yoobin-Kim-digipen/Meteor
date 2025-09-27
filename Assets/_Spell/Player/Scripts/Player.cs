using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 7f;
    public float turnSpeed = 720f; // 도/초

    [Header("Acceleration")]
    public float acceleration = 20f;         // 지상 가속
    public float deceleration = 30f;         // 지상 감속
    public float airAcceleration = 5f;       // 공중 가속
    public float airDeceleration = 0f;       // 공중 감속

    [Header("Gravity")]
    public float fallMultiplier = 3f;        // 조금 더 현실감 있는 중력 

    [Header("Jump")]
    public Transform groundCheck;            // 발바닥 위치에 빈 오브젝트 배치
    public LayerMask groundMask;             // Ground 레이어 설정
    public float jumpHeight = 1.6f;          // 목표 점프 높이(물리식으로 계산)
    public float groundCheckRadius = 0.25f;  // 발 위치 반경
    public float maxSnapFallSpeed = 1f;
    public float jumpCutMultiplier = 0.5f;   // 점프 컷 비율(0~1)

    // 점프 컷용, 점프키를 빨리 떼면 살짝 낮게(속도감)
    public bool jumpCut = true;              //코드 내에선 항상 true상태, inspector내에서 토글용도(체크 해제시 jumpcut 기능사라짐)
    public bool jumpReleaseQueued;

    // 점프 컷 안정화용(최소 유지시간 + 점프 직후 접지 무시)
    public float minJumpCutDelay = 0.06f;    // 게임 감각 보정 (최소 점프 시간)
    public float jumpUngroundGrace = 0.05f;   // 물리 판정 보정 (착지 오탐 방지) 
    public float jumpStartTime;
    public float ungroundedUntil;
    
    // 점프 큐와 바닥 상태
    public  bool jumpQueued;
    public bool isGrounded;
    private float castDist = 0.3f;                   // sphere cast에서 사용됨 / public 불필요 자세한 내용은 함수 주석확인
    Vector3 groundNormal = Vector3.up;       // 경사면 노멀 캐싱

    public Rigidbody Rigidbody { get { return rb; } }
    public Vector3 moveDir;                         // Update에서 받은 입력을 FixedUpdate에서 사용
    public bool wantRotate;
    private Rigidbody rb;
    private Camera cam;

    public PlayerStateMachine stateMachine;

    void Start()
    {
        // 마우스 위치안보이게 하기
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Awake()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        if (stateMachine == null)
        {
            Debug.LogError("PlayerStateMachine 컴포넌트가 없습니다!");
        }

        if (groundCheck == null) Debug.LogError("groundCheck가 할당되지 않았습니다!");
        cam = Camera.main; // 카메라 캐싱
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; // 중력 사용
        rb.isKinematic = false;

        // 보간을 이용해 렌더 프레임과 물리 틱이 어긋날때 생기는 미세 떨림 방지
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 캐릭이 회전할때는 y축만 쓰므로 X/Z 축 회전을 잠가서 캐릭터가 넘어지거나 기울어지지 않게 함
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        moveDir = Vector3.zero;
        readMoveInput(ref moveDir);

        // 점프 입력은 프레임 단위로 '한 번만' 감지 + 홀드/릴리즈
        if (Keyboard.current != null)
        {
            var space = Keyboard.current.spaceKey;

            if (space.wasPressedThisFrame)
            {
                jumpQueued = true;
            }

            if (space.wasReleasedThisFrame)
            {
                jumpReleaseQueued = true; // FixedUpdate에서 소비
            }
        }

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            wantRotate = true;
        }

        else
        {
            wantRotate = false;
        }

        if (stateMachine != null)
        {
            stateMachine.Update();
        }
    }

    void FixedUpdate()
    {
        // 바닥 체크(Overlap + SphereCast로 안정화)
        updateGroundState();

        // 점프 직후 일정 시간 접지 무시
        if (Time.time < ungroundedUntil)
        {
            isGrounded = false;
        }

        if (stateMachine != null)
        {
            stateMachine.FixedUpdate();
        }

        // 회전(카메라 Yaw 기준)
        if (wantRotate)
        {
            faceCameraYaw();
        }
    }

    // 카메라 기준으로 입력 읽기(ref 버전)
    void readMoveInput(ref Vector3 moveDir)
    {
        if (Keyboard.current == null) return;

        float x = 0f, z = 0f;
        if (Keyboard.current.aKey.isPressed)
        {
            x -= 1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            x += 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            z -= 1f;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            z += 1f;
        }

        Vector2 input = new Vector2(x, z);
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize(); // 대각선 보정
        }

        Vector3 forward = Vector3.forward, right = Vector3.right;
        if (cam != null)
        {
            forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;
        }

        moveDir = forward * input.y + right * input.x;
    }

    // 바닥 체크(Overlap + SphereCast)
    void updateGroundState()
    {
        bool wasGrounded = isGrounded; // 이전 값 저장

        isGrounded = false;
        groundNormal = Vector3.up; // 평지 구분

        if (groundCheck == null) return;

        const float skin = 0.02f;

        // 1. 이미 겹쳐 있나? (딱 붙어있는 평지)
        // Physics.CheckSphere(center, radius, layerMask, queryTriggerInteraction) <- 함수 원형

        bool overlap = Physics.CheckSphere(
            //플레이어 에게 상속 되어 있는 groundcheck 오브젝트 위치
            groundCheck.position,

            //구의 둘레에서 스킨, 즉 0.02f 만큼 뺀 값을 넣어 오차 안정화
            groundCheckRadius - skin,

            //레이어 필터
            groundMask,

            //지면 외의 트리거 무시 (*Ground Mask / 대상 Layer 확인 필수 제발 까먹으면 좆됨*)
            QueryTriggerInteraction.Ignore
        );

        if (overlap && rb.linearVelocity.y >= -maxSnapFallSpeed)
        {
            isGrounded = true;

            //딱붙었으면 평지니까 벡터up
            groundNormal = Vector3.up; // Overlap만으로는 노멀을 모르니 일단 Up
            return;
        }

        // 2. 아주 근접(아직 안 겹친 상태) → SphereCast로 보조
        /*/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
           1) 발 기준점(groundCheck)에서 위로 (반지름 + skin)만큼 띄운 위치를 시작점(origin)으로 잡는다.
           2) 그 위치에서 반지름 groundCheckRadius의 구를 아래 방향(Vector3.down)으로 castDist만큼 이동시키며 충돌을 찾는다.
           3) 무엇이든 맞으면 isGrounded = true로 접지 처리하고, 표면 노멀(hit.normal)을 groundNormal에 저장한다. 
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/
        Vector3 origin = groundCheck.position + Vector3.up * (groundCheckRadius + skin);

        // Physics.SphereCast(origin, radius, direction, out hit, maxDistance, layerMask, queryTriggerInteraction) <- 함수 원형
        if (Physics.SphereCast(

            // 위와 동일
            origin,
            groundCheckRadius,

            // 캐스트 방향: 아래로 검사
            Vector3.down,

            // 히트 결과: 맞은 표면의 정보 받기 
            out RaycastHit hit,

            // 최대 거리: 아래로 얼마까지 찾을지(스텝 높이 + 여유 권장)
            // 실질적으로 groundCheck 기준 감지 가능 간격 ≈ castDist - skin
            castDist,

            // 위와 동일
            groundMask,
            QueryTriggerInteraction.Ignore
        ) && rb.linearVelocity.y >= -maxSnapFallSpeed)
        {
            isGrounded = true;

            //SphereCast로 맞은 표면의 법선 벡터(hit.normal)를 “지면 노멀” 변수(groundNormal)에 저장
            //혹시 나중에 벽에 비볐을때 cast 오작동으로 grounded 상태가 될 수 있음. 추후 필요 시(버그 발생 시) 바닥과의 각도 계산 필요
            groundNormal = hit.normal;
        }
    }

    //플레이어 몸통 회전 관련 코드
    public void faceCameraYaw()
    {
        if (cam == null) return;

        //카메라 수직성분을 0으로 만들어서 수평 성분만 남김
        Vector3 forward = cam.transform.forward;
        forward.y = 0f;

        //카메라 수직으로 봤을때 수평 방향 벡터가 0일떄 생기는 버그 방지
        if (forward.sqrMagnitude < 1e-4f) return;

        //forward 방향을 바라보는 회전(Quaternion)을 만든다. Vector3.up은 회전의 위쪽 방향(롤 고정) 기준.
        Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);

        //현재 회전(rb.rotation)에서 목표 회전(targetRot) 쪽으로 회전
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
    }

    // ---------------------------------        Debug         ----------------------------------------------------------------------
    void OnGUI()
    {
        // 디버그 텍스트를 표시할 사각형 영역 설정 (x, y, width, height)
        Rect rect = new Rect(10, 10, 300, 200);
        // 텍스트 스타일 설정 (폰트 크기, 색상)
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        // 표시할 디버그 정보 문자열 생성
        string debugInfo =
    $"isGrounded: {isGrounded}\n" +
    $"Velocity: {rb.linearVelocity.ToString("F2")}\n" +
    $"jumpReleaseQueued: {jumpReleaseQueued}\n" +
    $"jumpCut: {jumpCut}\n" +
    $"vy: {rb.linearVelocity.y:F2}\n" +
    $"State: {stateMachine?.CurrentState.GetType().Name ?? "None"}"; // 현재 상태 표시!

        // 화면에 텍스트 표시
        GUI.Label(rect, debugInfo, style);
    }

    //아래는 기즈모 활성화 시 바닥 충돌판정 디버그 시각화 용도임 지워도 상관 없음
    void OnDrawGizmos() { DrawGroundGizmos(); }    //<-------------이거 주석 풀면 기즈모 활성화 시 바닥 체크 보임
    void DrawGroundGizmos()
    {
        if (groundCheck == null) return;

        // 여기 값은 FixedUpdate에서 쓰는 값들과 맞추면 실제와 동일하게 보입니다
        const float skin = 0.02f;

        Vector3 pos = groundCheck.position;

        // 1) Overlap(CheckSphere)
        bool overlap = Physics.CheckSphere(
            pos,
            groundCheckRadius - skin,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
        Gizmos.color = overlap ? Color.green : Color.red;
        Gizmos.DrawWireSphere(pos, groundCheckRadius - skin);

        // 2) SphereCast 경로
        // 안전한 시작점: 반경+스킨만큼 위에서 시작(겹침 방지)
        Vector3 origin = pos + Vector3.up * (groundCheckRadius + skin);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, groundCheckRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * castDist);
        Gizmos.DrawWireSphere(origin + Vector3.down * castDist, groundCheckRadius);

        // 실제 히트 표시
        if (Physics.SphereCast(
            origin, groundCheckRadius, Vector3.down,
            out RaycastHit hit, castDist, groundMask, QueryTriggerInteraction.Ignore))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.point, 0.03f);

            Gizmos.color = Color.magenta; // 표면 노멀
            Gizmos.DrawLine(hit.point, hit.point + hit.normal * 0.25f);
        }
    }
}