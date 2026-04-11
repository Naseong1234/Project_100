using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 채집물 종류
public enum ItemType { Wood, Leather, Fruit, Flower, Mushroom, Meat }

[System.Serializable]
public class CraftingRecipe
{
    public FurnitureType furnitureType;
    public int maxAmount; // 최대 제작 가능 수량
    public int reqWood = 0;
    public int reqLeather = 0;
}

public class SurvivalSystemManager : MonoBehaviour
{
    public static SurvivalSystemManager instance;

    // 아이템 수량이 변동될 때 UIManager에게 알려주기 위한 이벤트
    public event Action OnInventoryChanged;

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

    // UIManager에서 접근할 수 있도록 public으로 열어둡니다.
    public Dictionary<FurnitureType, CraftingRecipe> recipes = new Dictionary<FurnitureType, CraftingRecipe>();

    private bool isExploring = false;

    private void Awake()
    {
        if (instance == null) instance = this;

        InitializeInventory();
        InitializeRecipes();
    }

    private void InitializeInventory()
    {
        inventory[ItemType.Wood] = 50;
        inventory[ItemType.Leather] = 0;
        inventory[ItemType.Fruit] = 0;
        inventory[ItemType.Flower] = 0;
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

    // 아이템 획득/소비 (데이터 처리 후 UI 이벤트 호출)
    public void ModifyItem(ItemType type, int amount)
    {
        inventory[type] += amount;
        inventory[type] = Mathf.Clamp(inventory[type], 0, MAX_ITEM_CAPACITY);

        // 데이터가 바뀌었으니 UI를 업데이트 하라고 신호를 보냅니다.
        OnInventoryChanged?.Invoke();
    }

    // 실제 제작 시 호출할 함수 (재료 차감)
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



    // 생존/모험 로직
    public void StartNewDay(bool chooseExplore)
    {
        if (isExploring) return;
        if (chooseExplore) StartCoroutine(ExploreRoutine());
        else { MaintainBase(); EndDay(false); }
    }

    private IEnumerator ExploreRoutine()
    {
        isExploring = true;
        float exploreTime = 5f;
        float elapsedTime = 0f;
        float eventInterval = 1f;
        float nextEventTime = eventInterval;

        while (elapsedTime < exploreTime)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= nextEventTime)
            {
                TriggerRandomEncounter();
                nextEventTime += eventInterval;
            }
            yield return null;
        }

        isExploring = false;
        EndDay(true);
    }

    private void TriggerRandomEncounter()
    {
        int randomEncounter = UnityEngine.Random.Range(0, 6);
        switch (randomEncounter)
        {
            case 0: ModifyItem(ItemType.Wood, 1); ModifyItem(ItemType.Fruit, 1); break;
            case 1: ModifyItem(ItemType.Leather, 1); break;
            case 2: ModifyItem(ItemType.Mushroom, 1); break;
            case 3: ModifyItem(ItemType.Flower, 1); currentMental = Mathf.Clamp(currentMental + 2, 0, maxMental); break;
            case 4: ModifyItem(ItemType.Meat, 1); break;
        }
    }

    private void MaintainBase() { /* 내실 다지기 */ }

    private void EndDay(bool didExplore)
    {
        currentHealth -= 5;
        currentHunger -= 5;
        currentMental -= 5;

        if (didExplore) { currentHealth -= 5; currentHunger -= 5; }

        if (currentHunger >= 80) currentHealth += 15;
        else if (currentHunger >= 50) currentHealth += 10;
        else currentHealth += 5;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
        currentMental = Mathf.Clamp(currentMental, 0, maxMental);

        CheckGameOver();
        day++;
    }

    public void EatFood(ItemType foodType)
    {
        if (inventory[foodType] <= 0) return;

        switch (foodType)
        {
            case ItemType.Mushroom: ModifyItem(foodType, -1); currentHunger += 2; break;
            case ItemType.Fruit: ModifyItem(foodType, -1); currentHunger += 5; break;
            case ItemType.Meat: ModifyItem(foodType, -1); currentHunger += 10; break;
        }
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
    }

    private void CheckGameOver()
    {
        if (currentHealth <= 0) Debug.Log("체력이 0이 되어 사망했습니다.");
        else if (day > maxDay) Debug.Log("100일 생존에 성공했습니다!");
    }
}