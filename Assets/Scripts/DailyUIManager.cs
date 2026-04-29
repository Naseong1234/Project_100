using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DailyUIManager : MonoBehaviour
{
    private GameManager gameManager;

    [Header("필수 연결 세팅")]
    public Transform player;
    public Camera mainCamera;

    [Header("Status UI (인스펙터에서 연결해주세요)")]
    public TextMeshProUGUI hp_Text;
    public TextMeshProUGUI hunger_Text;
    public TextMeshProUGUI mantal_Text;
    public Image hp_Bar_Image;
    public Image hunger_Bar_Image;
    public Image mantal_Bar_Image;

    [Header("Inventory UI (인스펙터에서 연결해주세요)")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI leatherText;
    public TextMeshProUGUI flowerText;
    public TextMeshProUGUI fruitText;
    public TextMeshProUGUI mushroomText;
    public TextMeshProUGUI meatText;

    [Header("Crafting UI")]
    public TextMeshProUGUI craftingConditionText;

    [Header("제작(Crafting) 설정")]
    public float spawnDistance = 2.5f;
    private int typeIndex = 0;

    [Header("수정(Edit) 설정")]
    public LayerMask furnitureLayer;
    float rotSpeed = 1f; //카메라 돌리는 속도

    private bool isEditMode = false;
    private GameObject selectedFurniture = null;

    [Header("수정 모드 세부 상태")]
    public bool isMoveMode = false;
    public bool isRotateMode = false;
    public float moveSpeed = 0.01f; // 가구 이동 속도 

    // 터치가 UI에서 시작되었는지 추적하기 위한 변수
    private int validTouchId = -1;
    private bool isValidMouseDrag = false;



    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        GameObject gmObject = GameObject.Find("GameManager");

        if (gmObject != null)
        {
            gameManager = gmObject.GetComponent<GameManager>();

            gameManager.OnInventoryChanged += UpdateInventoryUI;
            gameManager.OnStatusChanged += UpdateStatusUI;

            UpdateInventoryUI();
            UpdateStatusUI();
        }
        else
        {
            Debug.LogError("씬에 'GameManager'라는 이름의 오브젝트가 없습니다!");
        }
    }
    void Update()
    {
        HandleScreenDrag();
    }

    private void OnDestroy()
    {
        // 구독 해제 시에도 캐싱된 gameManager 변수 사용
        if (gameManager != null)
        {
            gameManager.OnInventoryChanged -= UpdateInventoryUI;
            gameManager.OnStatusChanged -= UpdateStatusUI;
        }
    }

    public void UpdateStatusUI()
    {
        if (gameManager == null) return;

        if (hp_Text != null) hp_Text.text = gameManager.currentHealth.ToString();
        if (hunger_Text != null) hunger_Text.text = gameManager.currentHunger.ToString();
        if (mantal_Text != null) mantal_Text.text = gameManager.currentMental.ToString();

        if (hp_Bar_Image != null) hp_Bar_Image.fillAmount = (float)gameManager.currentHealth / gameManager.maxHealth;
        if (mantal_Bar_Image != null) mantal_Bar_Image.fillAmount = (float)gameManager.currentMental / gameManager.maxMental;
        if (hunger_Bar_Image != null) hunger_Bar_Image.fillAmount = (float)gameManager.currentHunger / gameManager.maxHunger;
    }

    public void UpdateInventoryUI()
    {
        if (gameManager == null) return;

        var inv = gameManager.inventory;
        if (woodText) woodText.text = $"{inv[ItemType.Wood]}";
        if (leatherText) leatherText.text = $"{inv[ItemType.Leather]}";
        if (flowerText) flowerText.text = $"{inv[ItemType.Flower]}";
        if (fruitText) fruitText.text = $"{inv[ItemType.Fruit]}";
        if (mushroomText) mushroomText.text = $"{inv[ItemType.Mushroom]}";
        if (meatText) meatText.text = $"{inv[ItemType.Meat]}";
    }


    public void ShowCraftingConditionUI(FurnitureType type)
    {
        if (gameManager == null || !gameManager.recipes.ContainsKey(type)) return;

        CraftingRecipe recipe = gameManager.recipes[type];
        var inv = gameManager.inventory;
        StringBuilder sb = new StringBuilder();

        int currentCount = FurnitureController.instance.GetPlayerFurnitureCount(type);

        if (recipe.reqWood > 0)
        {
            int currentWood = inv[ItemType.Wood];
            string ox = currentWood >= recipe.reqWood ? "O" : "X";
            sb.AppendLine($"Wood need : {recipe.reqWood} / current : {currentWood} = {ox}");
        }

        if (recipe.reqLeather > 0)
        {
            int currentLeather = inv[ItemType.Leather];
            string ox = currentLeather >= recipe.reqLeather ? "O" : "X";
            sb.AppendLine($"Leather need : {recipe.reqLeather} / current : {currentLeather} = {ox}");
        }

        string countOX = currentCount < recipe.maxAmount ? "O" : "X";
        sb.AppendLine($"Max Amount = {recipe.maxAmount} / current : {currentCount} = {countOX}");

        if (craftingConditionText != null)
        {
            craftingConditionText.text = sb.ToString();
        }
    }

    public void InputNumber(int num)
    {
        typeIndex = num;
        ShowCraftingConditionUI((FurnitureType)typeIndex);
    }

    public void CraftFurnitureAction()
    {
        if (gameManager == null) return;

        FurnitureType typeToCraft = (FurnitureType)typeIndex;
        gameManager.TryConsumeMaterials(typeToCraft);

        Vector3 spawnPos = player.position + player.forward * spawnDistance;
        spawnPos.y = player.position.y;

        FurnitureController.instance.CraftFurniture(typeToCraft, spawnPos);
        ShowCraftingConditionUI(typeToCraft);
    }

    public void EnterEditMode() { isEditMode = true; }
    public void ExitEditMode() { isEditMode = false; }
    public void CreateUI(GameObject UIinput) { UIinput.SetActive(true); }
    public void RemoveUI(GameObject UIinput) { UIinput.SetActive(false); }

    public void CreatTest(GameObject UIinput)
    {
        if (gameManager == null) return;

        FurnitureType typeToCraft = (FurnitureType)typeIndex;
        if (gameManager.PossibleTest(typeToCraft)) UIinput.SetActive(true);
        else UIinput.SetActive(false);
    }

    //  모드 전환 버튼 이벤트
    public void SetMoveMode()
    {
        Debug.Log("이동 모드 ON");
        isEditMode = true; // 수정 모드로 진입
        isMoveMode = true;
        isRotateMode = false;
    }

    public void SetRotateMode()
    {
        Debug.Log("회전 모드 ON");
        isEditMode = true; // 수정 모드로 진입
        isMoveMode = false;
        isRotateMode = true;
    }

    private void HandleScreenDrag()
    {
        Vector2 deltaPos = Vector2.zero;
        Vector2 screenPosition = Vector2.zero;
        bool isScreenDrag = false;
        bool isTouchBegan = false;
        bool isTouchEnded = false;

        //  모바일  환경 처리
        if (Input.touchCount > 0)
        {
            foreach (UnityEngine.Touch touch in Input.touches)
            {
                // 손가락을 처음 댔을 때만 UI 검사
                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    // 조이스틱 같은 UI가 아닌, '맨땅'을 눌렀을 때만 이 손가락 번호를 기억합니다.
                    if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        validTouchId = touch.fingerId;
                        screenPosition = touch.position;
                        isTouchBegan = true;
                        break;
                    }
                }
                else if (touch.fingerId == validTouchId && touch.phase == UnityEngine.TouchPhase.Moved)
                {
                    deltaPos = touch.deltaPosition;
                    screenPosition = touch.position;
                    isScreenDrag = true;
                    break;
                }
                else if (touch.fingerId == validTouchId && (touch.phase == UnityEngine.TouchPhase.Ended || touch.phase == UnityEngine.TouchPhase.Canceled))
                {
                    isTouchEnded = true;
                    validTouchId = -1;
                    break;
                }
            }
        }
        // 2. PC  환경 처리
        else if (Mouse.current != null)
        {
            // 마우스를 막 눌렀을 때
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // UI 위가 아니라면 유효한 드래그로 인정
                if (!IsPointerOverUI_Mouse())
                {
                    isValidMouseDrag = true;
                    screenPosition = Mouse.current.position.ReadValue();
                    isTouchBegan = true;
                }
            }
            // 누른 채로 움직일 때 (유효한 드래그일 때만)
            else if (Mouse.current.leftButton.isPressed)
            {
                if (isValidMouseDrag)
                {
                    deltaPos = Mouse.current.delta.ReadValue();
                    screenPosition = Mouse.current.position.ReadValue();
                    isScreenDrag = true;
                }
            }
            // 마우스를 뗐을 때 초기화
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (isValidMouseDrag)
                {
                    isTouchEnded = true;
                }
                isValidMouseDrag = false;
            }
        }

        if (isTouchEnded)
        {
            selectedFurniture = null;
        }

        if (isTouchBegan && isEditMode)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, furnitureLayer))
            {
                selectedFurniture = hitInfo.transform.gameObject;
            }
        }

        if (!isScreenDrag) return;

        // 평상시 모드 (!isEditMode) - 카메라 회전
        if (!isEditMode)
        {
            CameraController camController = mainCamera.GetComponent<CameraController>();
            if (camController != null)
            {
                camController.AddRotation(deltaPos.x * rotSpeed * 0.1f);
            }
            return;
        }

        // 수정 모드 (isEditMode) - 가구 조작
        if (isEditMode && selectedFurniture != null)
        {
            Transform hitObject = selectedFurniture.transform;

            if (isMoveMode)
            {
                Vector3 camRight = mainCamera.transform.right;
                camRight.y = 0;
                camRight.Normalize();

                Vector3 camForward = mainCamera.transform.forward;
                camForward.y = 0;
                camForward.Normalize();

                Vector3 moveDelta = (camRight * deltaPos.x + camForward * deltaPos.y) * moveSpeed;
                hitObject.position += moveDelta;
            }
            else if (isRotateMode)
            {
                // 이동한 마우스/터치 값만큼 회전량을 계산
                float rotationAmount = -(deltaPos.x * rotSpeed * 0.1f);

                // x, z축은 건드리지 않고, 월드(Space.World) Y축 기준으로만 회전
                hitObject.Rotate(0f, rotationAmount, 0f, Space.World);
            }
        }
    }
    // 마우스 전용 UI 검사기로 이름과 역할을 분리 (멀티 터치는 위에서 처리하므로)
    private bool IsPointerOverUI_Mouse()
    {
        if (EventSystem.current == null || Pointer.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Pointer.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    //  아이템 섭취 관련 기능
    public void EatFoodByName(string itemName)
    {
        // 입력한 문자열을 Enum으로 변환 시도
        if (Enum.TryParse(itemName, true, out ItemType parsedItem))
        {
            EatFood(parsedItem);
        }
        else
        {
            Debug.LogWarning($"[{itemName}]은(는) 올바른 ItemType이 아닙니다.");
        }
    }

    public void EatFood(ItemType foodType)
    {
        if (gameManager == null || gameManager.inventory[foodType] <= 0) return;

        switch (foodType)
        {
            case ItemType.Mushroom:
                gameManager.ModifyItem(foodType, -1);
                gameManager.currentHunger += 2;
                break;
            case ItemType.Fruit:
                gameManager.ModifyItem(foodType, -1);
                gameManager.currentHunger += 5;
                break;
            case ItemType.Meat:
                gameManager.ModifyItem(foodType, -1);
                gameManager.currentHunger += 10;
                break;
            case ItemType.Flower:
                gameManager.ModifyItem(foodType, -1);
                gameManager.currentHunger += 1;
                break;
        }

        // 최대 허기 수치를 넘지 않도록 제한
        gameManager.currentHunger = Mathf.Clamp(gameManager.currentHunger, 0, gameManager.maxHunger);

        gameManager.ForceUpdateUI();
    }




    public void Quit()
    {
        if (DataSaveManager.instance != null)
        {
            DataSaveManager.instance.SaveGameData();
        }
        else
        {
            Debug.LogError("DataSaveManager를 찾을 수 없어 저장을 건너뜁니다!");
        }

        // 즉시 끄지 않고, 0.2초 대기하는 코루틴 실행
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        // PlayerPrefs.Save()가 디스크에 파일 쓰기를 완료할 시간을 벌어줍니다.
        yield return new WaitForSeconds(0.2f);

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}