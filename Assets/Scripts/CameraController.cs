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

    public void AddRotation(float deltaX)
    {
        currentX += deltaX;
    }

    void LateUpdate()
    {
        //  좌우 회전값(Y축)만 담은 쿼터니언 계산
        Quaternion yRotation = Quaternion.Euler(0, currentX, 0);

        // 위치 적용
        transform.position = player.position + yRotation * offset;

        // 회전 적용
        transform.rotation = yRotation * initialRotation;
    }

}