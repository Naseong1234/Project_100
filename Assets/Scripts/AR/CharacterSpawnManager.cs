using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CharacterSpawnManager : MonoBehaviour
{
    public GameObject indicator;
    public GameObject myCharacter;
    GameObject placedObject = null;
    public float rotSpeed = -0.1f;
    public float relocationDistance = 1.0f;
    //미세한 카메라 위치변경시에도 모델링의 위치가 변동되는 민감함을 조정하기 위해,
    // 사용자가 화면을 터치했을때 모델링의 기존위치와 새로 배치될 위치간의 거리를 측정해 일정거리
    // 떨어져야만 재배치 되도록 수정
    ARRaycastManager arManager;
    List<ARRaycastHit> hitInfos = new List<ARRaycastHit>();

    // 생성 모드인지 판별하는 변수
    public bool isSpawnMode = false;

    void Start()
    {
        arManager = GetComponent<ARRaycastManager>();
        indicator.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        DetectGround();
    }

    // UI 스폰 버튼의 OnClick 이벤트에 연결할 함수
    public void OnSpawnButtonClicked()
    {
        // 버튼을 누를 때마다 true/false가 반전됨 (토글 기능)
        isSpawnMode = !isSpawnMode;
    }

    void DetectGround()
    {
        // isSpawnMode가 true일 때만 바닥을 인식하고 인디케이터를 표시함
        if (isSpawnMode)
        {
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            if (arManager.Raycast(screenCenter, hitInfos, TrackableType.Planes))
            {
                indicator.SetActive(true);

                indicator.transform.position = hitInfos[0].pose.position;
                indicator.transform.rotation = hitInfos[0].pose.rotation;
                indicator.transform.position += indicator.transform.up * 0.01f;
            }
            else
            {
                indicator.SetActive(false);
            }
        }
        else
        {
            // 스폰 모드가 아닐 때는 인디케이터를 강제로 끔
            indicator.SetActive(false);
        }
    }

    public void OnTouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (EventSystem.current.currentSelectedGameObject) // 터치한 오브젝트가 UI 오브젝트라면, 함수 종료
            {
                return;
            }

            // 인디케이터가 켜져 있고, 스폰 모드일 때만 터치 배치 작동
            if (indicator.activeInHierarchy && isSpawnMode)
            {
                // [핵심 해결 방법] 이미 소환된 캐릭터가 있고, 멀리 떨어져 있다면 파괴하고 새로 만듭니다.
                // 이는 "소환되자마자 사라지는" 문제를 해결하기 위한 가장 확실한 방법입니다.
                if (placedObject != null && Vector3.Distance(placedObject.transform.position, indicator.transform.position) > relocationDistance)
                {
                    Destroy(placedObject);
                    placedObject = null; // null로 만들어 다음 if문에서 새로 생성하도록 함
                }

                // 캐릭터 최초 생성 또는 재소환 (placedObject가 null인 경우에만 작동)
                if (placedObject == null)
                {
                    placedObject = Instantiate(myCharacter, indicator.transform.position, indicator.transform.rotation);

                    // [팁] 소환되자마자 물리 속도를 초기화해줍니다. (Start에서 해줘도 됩니다)
                    Rigidbody rb = placedObject.GetComponent<Rigidbody>();
                    if (rb != null) rb.linearVelocity = Vector3.zero;
                }
                else
                {
                    // 근처에 있다면, 그냥 인디케이터 위치로 즉시 이동만 시킬 수도 있습니다.
                    // 이 경우에도, 만약 비정상적인 물리 상태라면 속도를 초기화해줘야 "사라지는" 문제를 막을 수 있습니다.
                    // placedObject.transform.SetPositionAndRotation(indicator.transform.position, indicator.transform.rotation);
                    // Rigidbody rb = placedObject.GetComponent<Rigidbody>();
                    // if (rb != null) rb.velocity = Vector3.zero; // 물리 속도 초기화
                }

                // 캐릭터가 성공적으로 생성되거나 배치된 후 스폰 모드 끄기
                isSpawnMode = false;
                indicator.SetActive(false); // 즉시 인디케이터 숨김
            }
        }
    }
}