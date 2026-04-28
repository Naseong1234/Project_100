using UnityEngine;
using System.Collections;

public class ARPlayerController : MonoBehaviour
{
    private Joystick joystick;
    public static ARPlayerController instance;

    Rigidbody rb;
    public Animator animator;
    public float rotationSpeed = 10f;
    public float speed = 1f;
    private bool isGrounded = true;
    private Camera mainCam;

    [Header("채집 시스템")]
    public bool isGatheringMode = false;      // 10초간 활성화될 변수
    GameObject successUI;    // 채집 성공 UI (인스펙터에서 연결)
    private float collisionTimer = 0f;       // 접촉 시간을 잴 타이머
    private GameObject currentTarget = null; // 현재 접촉 중인 오브젝트

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        Time.timeScale = 1;
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;

        // 1. 조이스틱 찾기
        GameObject joystickObj = GameObject.Find("Fixed Joystick");
        if (joystickObj != null) joystick = joystickObj.GetComponent<Joystick>();

        // ==========================================
        // [수정된 부분] 비활성화된 successUI 찾기
        // ==========================================
        // 항상 켜져 있는 부모(Canvas)를 먼저 찾습니다.
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            // 부모의 transform.Find를 사용하면 꺼져있는 자식도 이름으로 찾을 수 있습니다.
            Transform successTransform = canvasObj.transform.Find("successUI");

            if (successTransform != null)
            {
                successUI = successTransform.gameObject;
                successUI.SetActive(false); // 확실하게 꺼줌
            }
        }

        // 혹시라도 못 찾았을 때를 대비한 디버그 로그
        if (successUI == null)
        {
            Debug.LogError("Canvas 아래에서 successUI를 찾을 수 없습니다!");
        }
    }

    void Update()
    {
        playerMove();
    }

    // ==========================================
    // 채집 버튼 이벤트 (UI 버튼에 연결하세요)
    // ==========================================
    public void OnGatheringButtonClicked()
    {
        if (!isGatheringMode)
        {
            StartCoroutine(GatheringModeRoutine());
        }
    }

    IEnumerator GatheringModeRoutine()
    {
        Debug.Log("채집 모드 시작 (10초)");
        isGatheringMode = true;
        yield return new WaitForSeconds(10f);
        isGatheringMode = false;
        Debug.Log("채집 모드 종료");

        // 모드가 끝나면 타이머 초기화
        collisionTimer = 0f;
        currentTarget = null;
    }

    // ==========================================
    // 물리 접촉 로직 (Trigger Stay)
    // ==========================================
    private void OnTriggerStay(Collider other)
    {
        // 채집 모드일 때만 작동
        if (!isGatheringMode) return;

        // 새로운 물체에 닿았거나 물체가 바뀐 경우 타이머 리셋
        if (currentTarget != other.gameObject)
        {
            currentTarget = other.gameObject;
            collisionTimer = 0f;
        }

        collisionTimer += Time.deltaTime;

        // 3초 이상 접촉 유지 시
        if (collisionTimer >= 3f)
        {
            CollectItem(other.gameObject);
            collisionTimer = 0f; // 획득 후 타이머 초기화
            currentTarget = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 범위를 벗어나면 초기화
        if (currentTarget == other.gameObject)
        {
            currentTarget = null;
            collisionTimer = 0f;
        }
    }

    void CollectItem(GameObject target)
    {
        string targetTag = target.tag;
        bool isCollected = false;

        // 태그에 따른 스위치문 처리
        switch (targetTag)
        {
            case "Tree":
                GameManager.instance.ModifyItem(ItemType.Wood, 2);
                isCollected = true;
                break;
            case "Animal":
                GameManager.instance.ModifyItem(ItemType.Meat, 2);
                GameManager.instance.ModifyItem(ItemType.Leather, 1);
                isCollected = true;
                break;
            case "Bushe":
                GameManager.instance.ModifyItem(ItemType.Fruit, 1);
                isCollected = true;
                break;
            case "Flower":
                GameManager.instance.ModifyItem(ItemType.Flower, 1);
                // 1. 정신력 수치 증가
                GameManager.instance.currentMental += 4;
                // 2. 최대 정신력(maxMental)을 넘지 않도록 안전장치 적용
                GameManager.instance.currentMental = Mathf.Clamp(GameManager.instance.currentMental, 0, GameManager.instance.maxMental);
                // 3. 변경된 수치가 화면(UI)에 즉시 반영되도록 신호 보내기
                GameManager.instance.ForceUpdateUI();

                isCollected = true;
                break;
            case "Mushroom":
                // 버섯도 아이템 타입의 Mushroom으로 연결 (사용자 요청에 따라 Flower로 할 수도 있음)
                GameManager.instance.ModifyItem(ItemType.Mushroom, 1);
                isCollected = true;
                break;
        }

        if (isCollected)
        {
            GameObject.Find("ItemGenerator").GetComponent<ItemGenerator>()?.DecreaseGatherableCount();

            // UI 표시 코루틴 실행
            StartCoroutine(ShowSuccessUI());

            // 채집된 오브젝트 파괴
            Destroy(target);
        }
    }

    IEnumerator ShowSuccessUI()
    {
        if (successUI != null)
        {
            successUI.SetActive(true);
            yield return new WaitForSeconds(2f);
            successUI.SetActive(false);
        }
    }

    // 기존 플레이어 이동 로직 (유지)
    void playerMove()
    {
        if (joystick == null) return;

        int groundMask = LayerMask.GetMask("Ground");
        Vector3 rayOrigin = transform.position + Vector3.up * 0.3f;
        float rayLength = 0.5f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength, groundMask);

        float h = joystick.Horizontal;
        float v = joystick.Vertical;

        if (h == 0 && v == 0)
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }

        Vector3 inputDir = new Vector3(h, 0f, v);
        float currentYVelocity = rb.linearVelocity.y;

        if (!isGrounded)
        {
            currentYVelocity += Physics.gravity.y * Time.deltaTime * 2f;
        }

        if (inputDir.magnitude >= 0.1f)
        {
            Vector3 camForward = mainCam.transform.forward;
            Vector3 camRight = mainCam.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * v + camRight * h).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            Vector3 moveVelocity = moveDir * speed;
            rb.linearVelocity = new Vector3(moveVelocity.x, currentYVelocity, moveVelocity.z);
            animator.SetBool("isWalking", true);
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, currentYVelocity, 0f);
            rb.angularVelocity = Vector3.zero;
            animator.SetBool("isWalking", false);
        }
    }
}