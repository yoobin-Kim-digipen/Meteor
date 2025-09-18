using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;
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
    bool jumpReleaseQueued;

    // 점프 컷 안정화용(최소 유지시간 + 점프 직후 접지 무시)
    public float minJumpCutDelay = 0.06f;    // 게임 감각 보정 (최소 점프 시간)
    public float jumpUngroundGrace = 0.05f;   // 물리 판정 보정 (착지 오탐 방지) 
    float jumpStartTime;
    float ungroundedUntil;
    
    // 점프 큐와 바닥 상태
    Vector3 groundNormal = Vector3.up;       // 경사면 노멀 캐싱
    float castDist = 0.3f;                   // sphere cast에서 사용됨 / public 불필요 자세한 내용은 함수 주석확인
    bool jumpQueued;
    bool isGrounded;

    Rigidbody rb;
    Camera cam;
    Vector3 moveDir;                         // Update에서 받은 입력을 FixedUpdate에서 사용
    bool wantRotate;

    void Start()
    {
        // 마우스 위치안보이게 하기
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Awake()
    {
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

        // 이동(가속/감속 기반, velocity 제어)
        applyMovementPhysics();

        // 낙하 중일 때만 중력 강화
        if (!isGrounded && rb.linearVelocity.y < 0f)
        {
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        }

        // 점프(물리에서 처리)
        if (jumpQueued)
        {
            jumpQueued = false;

            if (isGrounded)
            {
                // (물리식) 목표 높이 h에 도달할 초기 속도: v = sqrt(2gh)
                float vy = Mathf.Sqrt(Mathf.Max(0f, -2f * Physics.gravity.y * jumpHeight));

                Vector3 v = rb.linearVelocity;

                // 수직 성분 초기화(일관된 높이)
                v.y = 0f;

                rb.linearVelocity = v;

                // 위로만 즉시 속도 부여(질량 무관)
                rb.AddForce(Vector3.up * vy, ForceMode.VelocityChange);

                // 여기서 기록
                jumpStartTime = Time.time;
                ungroundedUntil = Time.time + jumpUngroundGrace;
            }
        }

        // 컷 처리
        bool canCutNow = jumpCut && rb.linearVelocity.y > 0f && (Time.time - jumpStartTime) >= minJumpCutDelay;
        if (canCutNow && jumpReleaseQueued)
        {
            jumpReleaseQueued = false; // 소비

            Vector3 v = rb.linearVelocity;
            v.y *= Mathf.Clamp01(jumpCutMultiplier);
            rb.linearVelocity = v;
        }

        // 회전(카메라 Yaw 기준)
        if (wantRotate)
        {
            faceCameraYaw();
        }

        // --- 착지 시 큐 초기화 (중요!) ---
        if (isGrounded)
        {
            jumpReleaseQueued = false; // 땅에 있을 땐 컷 플래그 항상 클리어
        }
    }

    // ---------------------------------       Update()       ----------------------------------------------------------------------

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

    // ---------------------------------     FixedUpdate()    ----------------------------------------------------------------------

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

    // 가속/감속 기반 이동 적용
    void applyMovementPhysics()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 v = rb.linearVelocity;

        // 평면벡터 만들기 (x축과 z축만 사용)
        Vector3 planar = new Vector3(v.x, 0f, v.z);

        // clamp사용으로 입력값 정규화
        float targetSpeed = moveSpeed * Mathf.Clamp01(moveDir.magnitude);
        Vector3 wishDir = (moveDir.sqrMagnitude > 1e-6f) ? moveDir.normalized : Vector3.zero;

        if (isGrounded)
        {
            if (wishDir != Vector3.zero)
            {
                // 지상: 입력 방향으로 '가속만' 더해줌(감속은 별도)
                // 현재 속도의 "wishDir 방향 성분" (투영값)
                // → 예: wishDir=전방, planar=전방 3 + 옆 1 → curAlong=3
                float curAlong = Vector3.Dot(planar, wishDir); // 입력 방향 성분 속도

                // → curAlong < target → 양수 (가속 필요), > target → 음수 (이미 빠름)
                float addNeeded = targetSpeed - curAlong; // 목표까지 필요한 증가량

                if (addNeeded > 0f)
                {
                    // 부드러운 가속을 위한 Min사용 급가속 방지
                    float add = Mathf.Min(acceleration * dt, addNeeded);
                    planar += wishDir * add;
                }
                else
                {
                    // 반대 방향 입력: 적당히 감속
                    float reduce = Mathf.Min(deceleration * dt, -addNeeded);
                    planar += wishDir * (-reduce);
                }

                // 최고 속도 살짝 클램프(넘치면 조금만 깎기)
                // 가속 후 magnitude가 targetSpeed 초과하면(옆 입력 등으로), 초과분만 deceleration*dt만큼 깎음.
                // normalized * (magnitude - cut): 방향 유지하면서 길이만 줄임 → "슬라이드" 방지
                if (planar.magnitude > targetSpeed)
                {
                    float over = planar.magnitude - targetSpeed;
                    float cut = Mathf.Min(deceleration * dt, over);
                    planar = planar.normalized * (planar.magnitude - cut);
                }
            }
            else
            {
                // 입력 없으면 서서히 감속
                // MoveTowards는 클램프 내장 → over-shoot 없음
                planar = Vector3.MoveTowards(planar, Vector3.zero, deceleration * dt);
            }

        }
        else
        {
            // 공중 로직은 관성 유지 + 입력 방향 가속
            if (wishDir != Vector3.zero)
            {
                float proj = Vector3.Dot(planar, wishDir);
                float addNeeded = targetSpeed - proj;
                if (addNeeded > 0f)
                {
                    float add = Mathf.Min(airAcceleration * dt, addNeeded);
                    planar += wishDir * add;
                }
                else if (addNeeded < 0f && airDeceleration > 0f)
                {
                    float reduce = Mathf.Min(airDeceleration * dt, -addNeeded);
                    planar += wishDir * (-reduce);
                }
            }
            else
            {
                if (airDeceleration > 0f)
                    planar = Vector3.MoveTowards(planar, Vector3.zero, airDeceleration * dt);
            }
        }

        v.x = planar.x;
        v.z = planar.z;
        rb.linearVelocity = v;
    }

    //플레이어 몸통 회전 관련 코드
    void faceCameraYaw()
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

    //아래는 기즈모 활성화 시 바닥 충돌판정 디버그 시각화 용도임 지워도 상관 없음
    //void OnDrawGizmos() { DrawGroundGizmos(); }    //<-------------이거 주석 풀면 기즈모 활성화 시 바닥 체크 보임
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