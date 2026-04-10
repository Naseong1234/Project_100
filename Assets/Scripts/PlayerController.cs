using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Joystick joystick; // 인스펙터에서 Fixed Joystick을 여기에 드래그해서 넣으세요.

    public static PlayerController instance;

    Rigidbody rb;
    public Animator animator;
    public Transform cameraTransform;
    public float rotationSpeed = 7f;

    float speed = 6;
    private bool isGrounded = true;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        playerMove();

    }


    void playerMove()
    {
        int groundMask = LayerMask.GetMask("Ground");
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        float rayLength = 1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength, groundMask);

        // 1. 조이스틱 입력 받기
        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        // 조이스틱 입력이 없을 때만(PC 테스트용) 키보드 입력을 받음
        if (h == 0 && v == 0)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 inputDir = new Vector3(h, 0f, v);

        // 2. 이동 로직 (관성 제거)
        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 direction = inputDir.normalized;

            // 회전 처리
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, Time.deltaTime * rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 이동 방향 계산 (카메라 기준)
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // [핵심] 속도를 직접 대입하여 가속도/관성 없이 즉시 이동
            Vector3 moveVelocity = moveDir * speed;
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

            animator.SetBool("isWalking", true);
        }
        else
        {
            // [핵심] 입력이 없으면 속도를 즉시 0으로 만들어 멈춤 (관성 제거)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            rb.angularVelocity = Vector3.zero; // 회전 관성도 제거

            animator.SetBool("isWalking", false);
        }
    }
}
