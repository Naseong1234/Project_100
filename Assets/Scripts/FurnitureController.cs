using System.Collections.Generic;
using UnityEngine;

public enum FurnitureType
{
    Barrel_Small, Barrel_Big, Barrel_drink,
    Tent_Small, Tent_Big, Tent_Rest,
    Boxs, Chair1, Chair2,
    Table_Small, Table_Big, Table_Round
}

[System.Serializable]
public class FurnitureSetup
{
    public FurnitureType type;
    public GameObject prefab;
    public int maxCount;
}

public class FurnitureController : MonoBehaviour
{
    [Header("가구 설정 (프리팹만 드래그해서 넣어주세요)")]
    public List<FurnitureSetup> furnitureSetups = new List<FurnitureSetup>();

    private Dictionary<FurnitureType, List<GameObject>> availablePool = new Dictionary<FurnitureType, List<GameObject>>();
    private Dictionary<FurnitureType, List<GameObject>> playerPool = new Dictionary<FurnitureType, List<GameObject>>();

    public static FurnitureController instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Reset()
    {
        furnitureSetups = new List<FurnitureSetup>
        {
            new FurnitureSetup { type = FurnitureType.Barrel_Small, maxCount = 1 },
            new FurnitureSetup { type = FurnitureType.Barrel_Big, maxCount = 1 },
            new FurnitureSetup { type = FurnitureType.Barrel_drink, maxCount = 1 },
            new FurnitureSetup { type = FurnitureType.Tent_Small, maxCount = 1 },
            new FurnitureSetup { type = FurnitureType.Tent_Big, maxCount = 1 },
            new FurnitureSetup { type = FurnitureType.Tent_Rest, maxCount = 1 },
            new FurnitureSetup { type = FurnitureType.Boxs, maxCount = 1 },
            new FurnitureSetup { type = FurnitureType.Chair1, maxCount = 4 },
            new FurnitureSetup { type = FurnitureType.Chair2, maxCount = 4 },
            new FurnitureSetup { type = FurnitureType.Table_Big, maxCount = 4 },
            new FurnitureSetup { type = FurnitureType.Table_Small, maxCount = 1 },
            new FurnitureSetup { type = FurnitureType.Table_Round, maxCount = 1 },
        };
    }

    void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        foreach (var setup in furnitureSetups)
        {
            availablePool[setup.type] = new List<GameObject>();
            playerPool[setup.type] = new List<GameObject>();

            for (int i = 0; i < setup.maxCount; i++)
            {
                if (setup.prefab == null) continue;

                GameObject obj = Instantiate(setup.prefab, transform);
                obj.SetActive(false);
                availablePool[setup.type].Add(obj);
            }
        }
    }

    // ==========================================
    //  DataSaveManager 전용 함수 (추출 및 복원)
    // ==========================================

    // 현재 플레이어가 지은 가구들의 위치/각도 데이터를 리스트로 만들어 반환합니다.
    public List<FurnitureSaveData> GetActiveFurnitureData()
    {
        List<FurnitureSaveData> dataList = new List<FurnitureSaveData>();

        foreach (var kvp in playerPool)
        {
            FurnitureType type = kvp.Key;
            foreach (GameObject obj in kvp.Value)
            {
                FurnitureSaveData data = new FurnitureSaveData();
                data.type = type;
                data.position = obj.transform.position;
                data.rotation = obj.transform.rotation.eulerAngles; // 직렬화를 위해 Euler 각도 사용
                dataList.Add(data);
            }
        }
        return dataList;
    }

    // 저장된 데이터를 받아와서 맵에 쫙 깔아줍니다.
    public void RestoreFurniture(List<FurnitureSaveData> savedDataList)
    {
        foreach (var data in savedDataList)
        {
            if (availablePool.ContainsKey(data.type) && availablePool[data.type].Count > 0)
            {
                GameObject objToRestore = availablePool[data.type][0];

                availablePool[data.type].RemoveAt(0);
                playerPool[data.type].Add(objToRestore);

                objToRestore.transform.position = data.position;
                objToRestore.transform.rotation = Quaternion.Euler(data.rotation);
                objToRestore.SetActive(true);
            }
        }
    }

    // ==========================================
    //  제작 및 관리 함수
    // ==========================================

    public void CraftFurniture(FurnitureType type, Vector3 spawnPosition)
    {
        if (!availablePool.ContainsKey(type) || availablePool[type].Count == 0) return;

        GameObject furnitureToCraft = availablePool[type][0];
        availablePool[type].RemoveAt(0);
        playerPool[type].Add(furnitureToCraft);

        furnitureToCraft.transform.position = spawnPosition;
        furnitureToCraft.SetActive(true);

        // --- 추가된 부분: 가구 제작 시 멘탈 30 증가 ---
        if (GameManager.instance != null)
        {
            GameManager.instance.currentMental += 30;
            // 최대 멘탈 수치(maxMental)를 넘지 않도록 제한
            GameManager.instance.currentMental = Mathf.Clamp(GameManager.instance.currentMental, 0, GameManager.instance.maxMental);

            // UI에 즉시 반영되도록 이벤트 호출
            GameManager.instance.ForceUpdateUI();
        }
    }

    public void ReturnFurniture(FurnitureType type, GameObject furnitureObj)
    {
        if (playerPool.ContainsKey(type) && playerPool[type].Contains(furnitureObj))
        {
            playerPool[type].Remove(furnitureObj);
            furnitureObj.SetActive(false);
            availablePool[type].Add(furnitureObj);
        }
    }

    public int GetPlayerFurnitureCount(FurnitureType type)
    {
        if (playerPool.ContainsKey(type)) return playerPool[type].Count;
        return 0;
    }

    // ==========================================
    //  개발자용 / 초기화용: 모든 가구 즉시 회수
    // ==========================================
    public void ClearAllFurniture()
    {
        // 플레이어가 지어둔 모든 가구(playerPool)를 순회합니다.
        foreach (var kvp in playerPool)
        {
            FurnitureType type = kvp.Key;
            List<GameObject> placedList = kvp.Value;

            // 리스트의 요소를 지우면서 반복문을 돌 때는 뒤에서부터(역순) 도는 것이 안전합니다.
            for (int i = placedList.Count - 1; i >= 0; i--)
            {
                GameObject obj = placedList[i];
                obj.SetActive(false); // 화면에서 안 보이게 끄기
                availablePool[type].Add(obj); // 대기 풀로 다시 넣기
            }

            // 플레이어 소유 리스트를 완전히 비웁니다.
            placedList.Clear();
        }
        Debug.Log("맵에 배치된 모든 가구가 즉시 치워졌습니다!");
    }
}