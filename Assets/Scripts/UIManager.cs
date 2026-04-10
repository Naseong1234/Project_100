using UnityEngine;
using UnityEngine.EventSystems; // UI 터치 충돌 방지용

public class UIManager : MonoBehaviour
{
    [Header("필수 연결 세팅")]
    public Transform player;      // 캐릭터 위치 기준점
    public Camera mainCamera;     // 레이를 쏠 메인 카메라

    [Header("제작(Crafting) 설정")]
    public float spawnDistance = 2.5f; // 캐릭터 앞 어느 정도 거리에 생성할지

    [Header("수정(Edit) 설정")]
    public LayerMask furnitureLayer; // 가구만 콕 집어서 선택하기 위한 레이어

    private bool isEditMode = false;           // 현재 수정 모드인지 상태 저장
    private GameObject selectedFurniture = null; // 현재 마우스로 잡고 있는 가구
    private Plane dragPlane;                   // Y축을 고정하고 드래그하기 위한 가상의 수학적 평면

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        // 1. 수정 모드가 켜져 있을 때만 마우스/터치 드래그 기능 작동
        if (isEditMode)
        {
            HandleFurnitureDrag();
        }
    }

    // ==========================================
    //  [기능 1] 가구 제작 버튼에 연결할 함수
    // ==========================================

    /// <summary>
    /// UI의 제작 버튼을 눌렀을 때 실행됩니다.
    /// 유니티 버튼 OnClick 이벤트에서는 Enum을 바로 넣기 어려우므로 int(숫자)로 받습니다.
    /// (예: 0 = Barrel_Small, 1 = Barrel_Big...)
    /// </summary>
    public void CraftFurnitureAction(int typeIndex)
    {
        // 숫자를 가구 타입 Enum으로 변환
        FurnitureType typeToCraft = (FurnitureType)typeIndex;

        // 플레이어 앞(forward) 방향으로 정해진 거리(spawnDistance)만큼 떨어진 위치 계산
        Vector3 spawnPos = player.position + player.forward * spawnDistance;

        // (선택) 가구가 공중에 뜨지 않게 Y축을 플레이어 발밑 높이로 대략 맞춰줍니다.
        spawnPos.y = player.position.y;

        // 만들어둔 FurnitureController를 불러와서 가구 생성!
        GameObject newFurniture = FurnitureController.instance.CraftFurniture(typeToCraft, spawnPos);

        if (newFurniture != null)
        {
            Debug.Log($"{typeToCraft} 가구가 캐릭터 앞에 성공적으로 생성되었습니다!");
        }
    }


    // ==========================================
    //  [기능 2] 수정 모드 토글 및 드래그 로직
    // ==========================================

    /// <summary>
    /// UI의 '수정' 버튼을 누를 때마다 모드가 켜지고 꺼집니다.
    /// </summary>
    public void ToggleEditMode()
    {
        isEditMode = !isEditMode;
        Debug.Log(isEditMode ? "수정 모드 ON" : "수정 모드 OFF");

        // 수정 모드를 끄면 잡고 있던 가구도 즉시 놓습니다.
        if (!isEditMode)
        {
            selectedFurniture = null;
        }
    }

    /// <summary>
    /// 화면 터치/마우스 클릭을 통해 가구를 잡고 이동시키는 핵심 로직입니다.
    /// </summary>
    private void HandleFurnitureDrag()
    {
        // [안전장치] UI 창(버튼, 패널 등)을 클릭하고 있다면 뒤에 있는 가구가 눌리지 않게 막습니다.
        if (EventSystem.current.IsPointerOverGameObject() ||
           (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)))
            return;

        // 1. 마우스/터치를 누르는 순간 (레이캐스트 쏘기)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 카메라에서 레이를 쏴서 '가구 레이어(furnitureLayer)'에 맞는 물체가 있는지 검사
            if (Physics.Raycast(ray, out hit, 100f, furnitureLayer))
            {
                selectedFurniture = hit.collider.gameObject;

                // [핵심 공식] 선택한 가구의 Y축(높이)을 뚫고 지나가는 무한한 가상의 바닥(Plane)을 하나 생성합니다.
                dragPlane = new Plane(Vector3.up, selectedFurniture.transform.position);
            }
        }

        // 2. 누른 채로 드래그 중일 때 (가구 이동)
        if (Input.GetMouseButton(0) && selectedFurniture != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            float distance;

            // 레이가 아까 만든 '가상의 바닥(Plane)'과 어느 위치에서 만나는지 계산
            if (dragPlane.Raycast(ray, out distance))
            {
                // 마우스가 위치한 곳의 3D 좌표를 얻습니다.
                Vector3 targetPos = ray.GetPoint(distance);

                // 가구의 위치를 업데이트합니다. (X와 Z는 마우스를 따라가되, Y는 원래 가구 높이 고정)
                selectedFurniture.transform.position = new Vector3(targetPos.x, selectedFurniture.transform.position.y, targetPos.z);
            }
        }

        // 3. 마우스/터치를 떼는 순간 (가구 놓기)
        if (Input.GetMouseButtonUp(0))
        {
            selectedFurniture = null;
        }
    }

    public void CreateUI(GameObject UIinput)
    {
        UIinput.SetActive(true);
    }

    public void RemoveUI(GameObject UIinput)
    {
        UIinput.SetActive(false);
    }



}