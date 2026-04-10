using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float rotationSpeed = 5f;

    private Vector3 offset;
    private Quaternion initialRotation; // [추가] 씬에서 설정한 초기 카메라 각도
    private float currentX = 0f;

    void Start()
    {
        // 1. 처음 시작할 때 플레이어와 카메라의 거리(간격)를 기억합니다.
        offset = transform.position - player.position;

        // 2. [추가] 처음 시작할 때 카메라가 바라보고 있는 완벽한 각도를 기억합니다.
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // 3. 입력 받기 (마우스 클릭 또는 터치 스와이프)
        if (Input.GetMouseButton(1))
        {
            if (!IsPointerOverUIObject())
            {
                currentX += Input.GetAxis("Mouse X") * rotationSpeed;
            }
        }

        // 4. 좌우 회전값(Y축)만 담은 쿼터니언 계산
        Quaternion yRotation = Quaternion.Euler(0, currentX, 0);

        // 5. 위치 적용 (원래 거리만큼 떨어지되, 회전된 위치로)
        transform.position = player.position + yRotation * offset;

        // 6. [핵심] 회전 적용 (LookAt 삭제)
        // 우리가 씬에서 예쁘게 맞춰둔 초기 각도(initialRotation)에다가, 
        // 방금 손가락으로 돌린 좌우 회전값(yRotation)을 더해(곱해) 줍니다.
        transform.rotation = yRotation * initialRotation;
    }

    private bool IsPointerOverUIObject()
    {
        if (EventSystem.current == null) return false;

        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        return EventSystem.current.IsPointerOverGameObject();
    }
}