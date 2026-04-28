using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ItemType { Wood, Leather, Flower, Fruit, Mushroom, Meat }

[System.Serializable]
public class CraftingRecipe
{
    public FurnitureType furnitureType;
    public int maxAmount;
    public int reqWood = 0;
    public int reqLeather = 0;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public event Action OnInventoryChanged;
    public event Action OnStatusChanged;

    [Header("Survival Stats")]
    public int day = 1;
    public int maxDay = 100;
    public int maxHealth = 200;
    public int currentHealth = 200;
    public int maxHunger = 200;
    public int currentHunger = 200;
    public int maxMental = 200;
    public int currentMental = 200;

    [Header("Inventory (Resources - Max 99)")]
    public Dictionary<ItemType, int> inventory = new Dictionary<ItemType, int>();
    public const int MAX_ITEM_CAPACITY = 99;

    public Dictionary<FurnitureType, CraftingRecipe> recipes = new Dictionary<FurnitureType, CraftingRecipe>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // 1. 여기서 GameManager가 파괴되지 않도록 설정 (이미 완벽합니다!)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeInventory();
        InitializeRecipes();
    }

    private void Start()
    {
        OnStatusChanged?.Invoke();
    }

    private void InitializeInventory()
    {
        inventory[ItemType.Wood] = 50;
        inventory[ItemType.Leather] = 0;
        inventory[ItemType.Flower] = 0;
        inventory[ItemType.Fruit] = 0;
        inventory[ItemType.Mushroom] = 0;
        inventory[ItemType.Meat] = 0;
    }

    private void InitializeRecipes()
    {
        recipes.Add(FurnitureType.Barrel_Small, new CraftingRecipe { furnitureType = FurnitureType.Barrel_Small, maxAmount = 1, reqWood = 10 });
        recipes.Add(FurnitureType.Barrel_Big, new CraftingRecipe { furnitureType = FurnitureType.Barrel_Big, maxAmount = 1, reqWood = 15 });
        recipes.Add(FurnitureType.Barrel_drink, new CraftingRecipe { furnitureType = FurnitureType.Barrel_drink, maxAmount = 1, reqWood = 10 });
        recipes.Add(FurnitureType.Tent_Small, new CraftingRecipe { furnitureType = FurnitureType.Tent_Small, maxAmount = 1, reqWood = 5, reqLeather = 10 });
        recipes.Add(FurnitureType.Tent_Big, new CraftingRecipe { furnitureType = FurnitureType.Tent_Big, maxAmount = 1, reqWood = 10, reqLeather = 15 });
        recipes.Add(FurnitureType.Tent_Rest, new CraftingRecipe { furnitureType = FurnitureType.Tent_Rest, maxAmount = 1, reqWood = 10, reqLeather = 10 });
        recipes.Add(FurnitureType.Boxs, new CraftingRecipe { furnitureType = FurnitureType.Boxs, maxAmount = 1, reqWood = 20 });
        recipes.Add(FurnitureType.Chair1, new CraftingRecipe { furnitureType = FurnitureType.Chair1, maxAmount = 4, reqWood = 10 });
        recipes.Add(FurnitureType.Chair2, new CraftingRecipe { furnitureType = FurnitureType.Chair2, maxAmount = 4, reqWood = 15 });
        recipes.Add(FurnitureType.Table_Small, new CraftingRecipe { furnitureType = FurnitureType.Table_Small, maxAmount = 1, reqWood = 20 });
        recipes.Add(FurnitureType.Table_Big, new CraftingRecipe { furnitureType = FurnitureType.Table_Big, maxAmount = 1, reqWood = 30 });
        recipes.Add(FurnitureType.Table_Round, new CraftingRecipe { furnitureType = FurnitureType.Table_Round, maxAmount = 1, reqWood = 20 });
    }

    public void ModifyItem(ItemType type, int amount)
    {
        inventory[type] += amount;
        inventory[type] = Mathf.Clamp(inventory[type], 0, MAX_ITEM_CAPACITY);
        OnInventoryChanged?.Invoke();
    }

    public void TryConsumeMaterials(FurnitureType type)
    {
        CraftingRecipe recipe = recipes[type];
        ModifyItem(ItemType.Wood, -recipe.reqWood);
        ModifyItem(ItemType.Leather, -recipe.reqLeather);
    }

    public bool PossibleTest(FurnitureType type)
    {
        CraftingRecipe recipe = recipes[type];

        if (inventory[ItemType.Wood] < recipe.reqWood) return false;
        if (inventory[ItemType.Leather] < recipe.reqLeather) return false;
        if (FurnitureController.instance.GetPlayerFurnitureCount(type) >= recipe.maxAmount) return false;

        return true;
    }

    public void EndDay() // 나중에 밤 상태에서 Next를 누를 때 호출하는 걸로 이걸 연결하면 됨
    {
        currentHealth -= 5;
        currentHunger -= 5;
        currentMental -= 5;

        if (currentHunger >= 80) currentHealth += 15;
        else if (currentHunger >= 50) currentHealth += 10;
        else currentHealth += 5;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentMental = Mathf.Clamp(currentMental, 0, maxMental);

        OnStatusChanged?.Invoke();
        CheckGameOver();
        day++;
    }

    // 인스펙터 버튼 연결용 함수
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
        if (inventory[foodType] <= 0) return;

        switch (foodType)
        {
            case ItemType.Mushroom: ModifyItem(foodType, -1); currentHunger += 2; break;
            case ItemType.Fruit: ModifyItem(foodType, -1); currentHunger += 5; break;
            case ItemType.Meat: ModifyItem(foodType, -1); currentHunger += 10; break;
            case ItemType.Flower: ModifyItem(foodType, -1); currentHunger += 1; break;
        }
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        OnStatusChanged?.Invoke();
    }

    private void CheckGameOver()
    {
        if (currentHealth <= 0) Debug.Log("체력이 0이 되어 사망했습니다.");
        else if (day > maxDay) Debug.Log("100일 생존에 성공했습니다!");
    }

    // DataSaveManager 등 외부에서 강제로 UI 갱신 이벤트를 발생시킬 때 사용합니다.
    public void ForceUpdateUI()
    {
        OnInventoryChanged?.Invoke();
        OnStatusChanged?.Invoke();
    }
}