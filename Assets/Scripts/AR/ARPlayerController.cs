using UnityEngine;

public class ARPlayerController : MonoBehaviour
{
    private Joystick joystick;
    public static ARPlayerController instance;

    Rigidbody rb;
    public Animator animator;
    public float rotationSpeed = 10f; // 부드러운 턴을 위해 수치를 조금 높이는 걸 추천해

    public float speed = 1f;
    private bool isGrounded = true;

    // 카메라의 위치와 회전값을 가져오기 위한 변수
    private Camera mainCam;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        Time.timeScale = 1;
        rb = GetComponent<Rigidbody>();

        // 씬 시작 시 메인 카메라 캐싱
        mainCam = Camera.main;

        GameObject joystickObj = GameObject.Find("Fixed Joystick");
        if (joystickObj != null)
        {
            joystick = joystickObj.GetComponent<Joystick>();
        }
        else
        {
            Debug.LogError("씬에서 'Fixed Joystick' 오브젝트를 찾을 수 없습니다! 이름을 확인해주세요.");
        }
    }

    void Update()
    {
        playerMove();
    }

    void playerMove()
    {
        if (joystick == null) return;

        // 1. 땅 인식 로직 (AR Plane의 레이어가 "Ground"인지 반드시 확인!)
        int groundMask = LayerMask.GetMask("Ground");
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        float rayLength = 0.5f; // 레이캐스트 길이를 살짝 줄여서 바닥 판정을 더 엄격하게 함
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength, groundMask);

        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        if (h == 0 && v == 0)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 inputDir = new Vector3(h, 0f, v);

        // [핵심 1] 현재 리지드바디의 Y축 속도를 가져옴 (중력 보존)
        float currentYVelocity = rb.linearVelocity.y;

        // [핵심 2] 바닥을 벗어났다면 강제로 추가 중력을 가해 허공을 걷지 않고 떨어지게 만듦
        if (!isGrounded)
        {
            currentYVelocity += Physics.gravity.y * Time.deltaTime * 2f;
        }

        if (inputDir.magnitude >= 0.1f)
        {
            // [핵심 3] 카메라(스마트폰)가 바라보는 방향을 기준으로 벡터 계산
            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;

            // Y축(위아래) 값은 무시하고 평면 이동만 하도록 만듦
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // 조이스틱 입력과 카메라 방향을 곱해서 최종 이동 방향(moveDir) 산출
            Vector3 moveDir = (camForward * v + camRight * h).normalized;

            // 캐릭터가 자신이 이동하는 방향을 자연스럽게 바라보도록 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // 속도 적용 (이동 속도 + 계산된 Y축 중력 속도)
            Vector3 moveVelocity = moveDir * speed;
            rb.linearVelocity = new Vector3(moveVelocity.x, currentYVelocity, moveVelocity.z);

            animator.SetBool("isWalking", true);
        }
        else
        {
            // 입력이 없으면 X, Z축 이동만 멈추고 Y축(낙하 속도)은 그대로 유지
            rb.linearVelocity = new Vector3(0f, currentYVelocity, 0f);
            rb.angularVelocity = Vector3.zero;

            animator.SetBool("isWalking", false);
        }
    }
}