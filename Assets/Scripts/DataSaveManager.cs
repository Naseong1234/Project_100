using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FurnitureSaveData
{
    public FurnitureType type;
    public Vector3 position;
    public Vector3 rotation;
}

[Serializable]
public class GameSaveData
{
    public int day;
    public int currentHealth;
    public int currentHunger;
    public int currentMental;

    public int wood;
    public int leather;
    public int flower;
    public int fruit;
    public int mushroom;
    public int meat;

    public List<FurnitureSaveData> placedFurniture = new List<FurnitureSaveData>();
}

public class DataSaveManager : MonoBehaviour
{
    public static DataSaveManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        //ResetGameData();
        LoadGameData();
    }

    public void SaveGameData()
    {
        GameSaveData data = new GameSaveData();

        // 탐험씬에서 저장할 때 일상씬의 가구 데이터가 날아가지 않도록, 기존 세이브를 먼저 덮어씌우기
        if (PlayerPrefs.HasKey("GameSave"))
        {
            string existingJson = PlayerPrefs.GetString("GameSave");
            data = JsonUtility.FromJson<GameSaveData>(existingJson);
        }

        data.day = GameManager.instance.day;
        data.currentHealth = GameManager.instance.currentHealth;
        data.currentHunger = GameManager.instance.currentHunger;
        data.currentMental = GameManager.instance.currentMental;

        data.wood = GameManager.instance.inventory[ItemType.Wood];
        data.leather = GameManager.instance.inventory[ItemType.Leather];
        data.flower = GameManager.instance.inventory[ItemType.Flower];
        data.fruit = GameManager.instance.inventory[ItemType.Fruit];
        data.mushroom = GameManager.instance.inventory[ItemType.Mushroom];
        data.meat = GameManager.instance.inventory[ItemType.Meat];

        if (FurnitureController.instance != null)
        {
            data.placedFurniture = FurnitureController.instance.GetActiveFurnitureData();
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("GameSave", json);
        PlayerPrefs.Save();

    }

    public void LoadGameData()
    {
        if (!PlayerPrefs.HasKey("GameSave")) return;

        string json = PlayerPrefs.GetString("GameSave");
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        GameManager.instance.day = data.day;
        GameManager.instance.currentHealth = data.currentHealth;
        GameManager.instance.currentHunger = data.currentHunger;
        GameManager.instance.currentMental = data.currentMental;

        GameManager.instance.inventory[ItemType.Wood] = data.wood;
        GameManager.instance.inventory[ItemType.Leather] = data.leather;
        GameManager.instance.inventory[ItemType.Flower] = data.flower;
        GameManager.instance.inventory[ItemType.Fruit] = data.fruit;
        GameManager.instance.inventory[ItemType.Mushroom] = data.mushroom;
        GameManager.instance.inventory[ItemType.Meat] = data.meat;

        // 체력, 배고픔 등 인벤토리 UI 갱신
        GameManager.instance.ForceUpdateUI();

        // 데이터 로드가 끝나는 즉시 날짜 UI를 진짜 데이터로 덮어씌우기
        DayManager dayManager = FindFirstObjectByType<DayManager>();
        if (dayManager != null && dayManager.Day_Text != null)
        {
            dayManager.Day_Text.text = $"Day - {data.day}";
        }

        // 가구 매니저 복원
        if (FurnitureController.instance != null)
        {
            FurnitureController.instance.RestoreFurniture(data.placedFurniture);
        }
    }

    [ContextMenu(" 개발자용: 모든 데이터 초기화 (Reset)")]
    public void ResetGameData()
    {
        PlayerPrefs.DeleteKey("GameSave");
        PlayerPrefs.Save();

        //  GameManager 스탯 및 아이템 초기화
        if (GameManager.instance != null)
        {
            GameManager.instance.day = 1;
            GameManager.instance.currentHealth = 150;
            GameManager.instance.currentHunger = 150;
            GameManager.instance.currentMental = 150;

            GameManager.instance.inventory[ItemType.Wood] = 50;
            GameManager.instance.inventory[ItemType.Leather] = 50;
            GameManager.instance.inventory[ItemType.Flower] = 0;
            GameManager.instance.inventory[ItemType.Fruit] = 0;
            GameManager.instance.inventory[ItemType.Mushroom] = 0;
            GameManager.instance.inventory[ItemType.Meat] = 10;

            GameManager.instance.ForceUpdateUI();
        }

        // 일상씬에 가구 컨트롤러가 있다면, 화면에 보이는 가구도 즉시 치워버림
        if (FurnitureController.instance != null)
        {
            FurnitureController.instance.ClearAllFurniture();
        }

    }
    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    // 스마트폰 환경 필수: 전화가 오거나 홈 버튼을 눌러 앱이 뒤로 내려갈 때 자동으로 불리는 함수
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) // 앱이 백그라운드로 내려가서 일시정지 상태가 됨
        {
            SaveGameData();
        }
    }

}