using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float rotationSpeed = 5f;

    private Vector3 offset;
    private Quaternion initialRotation;
    private float currentX = 0f;

    void Start()
    {
        offset = transform.position - player.position;
        initialRotation = transform.rotation;
    }

    //[핵심 추가] 외부(DailyUIManager)에서 스와이프 값을 받아 회전시키는 함수
    public void AddRotation(float deltaX)
    {
        currentX += deltaX;
    }

    void LateUpdate()
    {
        //  [삭제할 부분] 
        // 이전에 PC 테스트용으로 넣었던 if (Application.isEditor && Input.GetMouseButton(0)) 블록을 통째로 지워주세요!
        // 이 녀석이 수정 모드에서도 무단으로 카메라를 돌리던 범인입니다.

        // 4. 좌우 회전값(Y축)만 담은 쿼터니언 계산
        Quaternion yRotation = Quaternion.Euler(0, currentX, 0);

        // 5. 위치 적용
        transform.position = player.position + yRotation * offset;

        // 6. 회전 적용
        transform.rotation = yRotation * initialRotation;
    }

}