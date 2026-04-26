using UnityEngine;

public class ARPlayerController : MonoBehaviour
{
    // 외부에서 드래그할 필요 없도록 private으로 변경 (또는 [HideInInspector] public 사용)
    private Joystick joystick;
    public static ARPlayerController instance;

    Rigidbody rb;
    public Animator animator;
    public float rotationSpeed = 7f;

    float speed = 6;

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

        // 1. 하이어라키에서 "Fixed Joystick"이라는 이름의 오브젝트를 찾음
        GameObject joystickObj = GameObject.Find("Fixed Joystick");

        // 2. 오브젝트를 성공적으로 찾았다면 Joystick 컴포넌트를 가져옴
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
        // 조이스틱을 아직 찾지 못했다면 이동 로직을 실행하지 않음 (NullReferenceException 방지)
        if (joystick == null) return;

        // 땅 인식 로직 (AR Plane Prefab의 레이어가 "Ground"로 설정되어 있어야 함)
        int groundMask = LayerMask.GetMask("Ground");
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        float rayLength = 1f;
        // 1. 조이스틱 입력 받기
        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        // PC 테스트용 키보드 입력
        if (h == 0 && v == 0)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 inputDir = new Vector3(h, 0f, v);

        // 2. 이동 로직 (카메라 독립, 월드 기준)
        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 direction = inputDir.normalized;

            // 카메라의 eulerAngles.y를 더하지 않고, 조이스틱 방향 자체를 타겟 각도로 사용
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // 자연스러운 회전 처리
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 입력 방향 그대로 속도로 변환 (가속도/관성 없이 즉시 이동)
            Vector3 moveVelocity = direction * speed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

            animator.SetBool("isWalking", true);
        }
        else
        {
            // 입력이 없으면 즉시 정지 (관성 제거)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.angularVelocity = Vector3.zero;

            animator.SetBool("isWalking", false);
        }
    }
}