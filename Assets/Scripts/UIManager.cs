using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro; // 추가됨!
using System.Text; // 추가됨!
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("필수 연결 세팅")]
    public Transform player;
    public Camera mainCamera;

    [Header("Inventory UI (인스펙터에서 연결해주세요)")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI leatherText;
    public TextMeshProUGUI fruitText;
    public TextMeshProUGUI flowerText;
    public TextMeshProUGUI mushroomText;
    public TextMeshProUGUI meatText;

    [Header("Crafting UI")]
    public TextMeshProUGUI craftingConditionText;

    [Header("제작(Crafting) 설정")]
    public float spawnDistance = 2.5f;
    private int typeIndex = 0;

    [Header("수정(Edit) 설정")]
    public LayerMask furnitureLayer;
    public float rotSpeed = 0.5f;

    private bool isEditMode = false;
    private GameObject selectedFurniture = null;
    private Plane dragPlane;
    private Vector2 pointerPosition;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // SurvivalSystemManager의 이벤트에 연결하여, 아이템이 바뀔 때마다 자동으로 UI가 갱신되게 만듭니다.
        if (SurvivalSystemManager.instance != null)
        {
            SurvivalSystemManager.instance.OnInventoryChanged += UpdateInventoryUI;
            UpdateInventoryUI(); // 게임 시작 시 초기화
        }
    }

    private void OnDestroy()
    {
        // 씬이 넘어가거나 오브젝트가 파괴될 때 이벤트 연결을 해제해줍니다. (메모리 누수 방지)
        if (SurvivalSystemManager.instance != null)
        {
            SurvivalSystemManager.instance.OnInventoryChanged -= UpdateInventoryUI;
        }
    }

    // ==========================================
    //  [기능 1] 인벤토리 및 조건 UI 갱신 (추가된 부분)
    // ==========================================

    public void UpdateInventoryUI()
    {
        var inv = SurvivalSystemManager.instance.inventory;
        if (woodText) woodText.text = $"{inv[ItemType.Wood]}";
        if (leatherText) leatherText.text = $"{inv[ItemType.Leather]}";
        if (fruitText) fruitText.text = $"{inv[ItemType.Fruit]}";
        if (flowerText) flowerText.text = $"{inv[ItemType.Flower]}";
        if (mushroomText) mushroomText.text = $"{inv[ItemType.Mushroom]}";
        if (meatText) meatText.text = $"{inv[ItemType.Meat]}";
    }

    public void ShowCraftingConditionUI(FurnitureType type)
    {
        if (!SurvivalSystemManager.instance.recipes.ContainsKey(type)) return;

        CraftingRecipe recipe = SurvivalSystemManager.instance.recipes[type];
        var inv = SurvivalSystemManager.instance.inventory;
        StringBuilder sb = new StringBuilder();

        // 1. 나무 조건 체크
        if (recipe.reqWood > 0)
        {
            int currentWood = inv[ItemType.Wood];
            string ox = currentWood >= recipe.reqWood ? "O" : "X";
            sb.AppendLine($"Wood need : {recipe.reqWood} / current : {currentWood} = {ox}");
        }

        // 2. 가죽 조건 체크
        if (recipe.reqLeather > 0)
        {
            int currentLeather = inv[ItemType.Leather];
            string ox = currentLeather >= recipe.reqLeather ? "O" : "X";
            sb.AppendLine($"Leather need : {recipe.reqLeather} / current : {currentLeather} = {ox}");
        }

        // 3. 수량 조건 체크
        int currentCount = FurnitureController.instance.GetPlayerFurnitureCount(type);
        string countOX = currentCount < recipe.maxAmount ? "O" : "X";
        sb.AppendLine($"Max Amount = {recipe.maxAmount} / current : {currentCount} = {countOX}");

        if (craftingConditionText != null)
        {
            craftingConditionText.text = sb.ToString();
        }
    }

    // ==========================================
    //  [기능 2] 버튼 입력 및 제작 수행
    // ==========================================

    public void InputNumber(int num)
    {
        typeIndex = num;
        // 번호(가구 종류)를 선택하면, 즉시 해당 가구의 조건 텍스트를 UI에 띄워줍니다!
        ShowCraftingConditionUI((FurnitureType)typeIndex);
    }

    public void CraftFurnitureAction()
    {
        FurnitureType typeToCraft = (FurnitureType)typeIndex;
        SurvivalSystemManager.instance.TryConsumeMaterials(typeToCraft);

        // 재료 소모에 성공했을 때만 생성!
        Vector3 spawnPos = player.position + player.forward * spawnDistance;
        spawnPos.y = player.position.y;

        FurnitureController.instance.CraftFurniture(typeToCraft, spawnPos);

        // 제작 후 조건 텍스트 한 번 더 갱신 (수량이 늘어났으므로)
        ShowCraftingConditionUI(typeToCraft);
    }

    public void EnterEditMode()
    {
        isEditMode = true;
    }
    public void ExitEditMode()
    {
        isEditMode = false;
    }

    public void CreateUI(GameObject UIinput) { UIinput.SetActive(true); }
    public void RemoveUI(GameObject UIinput) { UIinput.SetActive(false); }
    public void CreatTest(GameObject UIinput)
    {
        FurnitureType typeToCraft = (FurnitureType)typeIndex;

        if (SurvivalSystemManager.instance.PossibleTest(typeToCraft))
        {
            UIinput.SetActive(true);
        }
        else
        {
            UIinput.SetActive(false);

        }
    }


    // ==========================================
    //  [기능 3] Input System 이벤트 
    // ==========================================

    public void OnPointerPosition(InputAction.CallbackContext context)
    {
        if (!isEditMode) return;

        pointerPosition = context.ReadValue<Vector2>();

        if (selectedFurniture != null)
        {
            bool isTwoFingerTouch = Touchscreen.current != null && Touchscreen.current.touches[1].press.isPressed;

            if (!isTwoFingerTouch)
            {
                Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
                if (dragPlane.Raycast(ray, out float distance))
                {
                    Vector3 targetPos = ray.GetPoint(distance);
                    selectedFurniture.transform.position = new Vector3(targetPos.x, selectedFurniture.transform.position.y, targetPos.z);
                }
            }
        }
    }

    public void OnTouchPress(InputAction.CallbackContext context)
    {
        if (!isEditMode) return;

        if (context.started)
        {
            if (IsPointerOverUI()) return;

            Vector2 pressPos = Pointer.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(pressPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, furnitureLayer))
            {
                selectedFurniture = hit.collider.gameObject;
                dragPlane = new Plane(Vector3.up, selectedFurniture.transform.position);
            }
        }
        else if (context.canceled)
        {
            selectedFurniture = null;
        }
    }

    public void OnSwipeRotate(InputAction.CallbackContext context)
    {
        if (isEditMode && selectedFurniture != null && context.performed)
        {
            Vector2 delta = context.ReadValue<Vector2>();
            selectedFurniture.transform.Rotate(Vector3.up, -delta.x * rotSpeed, Space.World);
        }
    }

    private bool IsPointerOverUI()
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
}