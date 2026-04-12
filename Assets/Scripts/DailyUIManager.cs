using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;

public class DailyUIManager : MonoBehaviour
{
    // 2. 인스펙터 연결이나 싱글톤 대신, 스크립트 내부에서 찾아서 사용할 변수 선언
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
    public float rotSpeed = 0.5f;

    private bool isEditMode = false;
    private GameObject selectedFurniture = null;
    private Plane dragPlane;
    private Vector2 pointerPosition;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // 3. 씬에서 이름이 "GameManager"인 오브젝트를 찾아, 그 안에 있는 GameManager 스크립트를 가져옵니다.
        // 이 방법은 씬이 넘어간 직후에도 안전하게 기존 유지된 매니저를 다시 찾아올 수 있습니다.
        GameObject gmObject = GameObject.Find("GameManager");

        if (gmObject != null)
        {
            gameManager = gmObject.GetComponent<GameManager>();

            // 찾은 gameManager 변수를 바탕으로 이벤트 구독
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

    private void OnDestroy()
    {
        // 구독 해제 시에도 캐싱된 gameManager 변수 사용
        if (gameManager != null)
        {
            gameManager.OnInventoryChanged -= UpdateInventoryUI;
            gameManager.OnStatusChanged -= UpdateStatusUI;
        }
    }

    // ==========================================
    //  UI 갱신 함수들
    // ==========================================

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

    // ==========================================
    //  제작 (Crafting) 관련 기능
    // ==========================================

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

    // ==========================================
    //  Input System 이벤트 
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