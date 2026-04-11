using System.Collections.Generic;
using UnityEngine;

// 가구의 종류를 정의합니다.
public enum FurnitureType
{
    Barrel_Small, Barrel_Big, Barrel_drink,
    Tent_Small, Tent_Big, Tent_Rest,
    Boxs, Chair1,Chair2, 
    Table_Small, Table_Big, Table_Round
}

// 인스펙터에서 프리팹과 최대 개수를 설정하기 위한 직렬화 클래스입니다.
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

    // 1. 대기 중인 가구 리스트 (기존에 만든 리스트 풀)
    private Dictionary<FurnitureType, List<GameObject>> availablePool = new Dictionary<FurnitureType, List<GameObject>>();

    // 2. 플레이어가 제작한 가구 리스트 (플레이어 리스트 풀)
    private Dictionary<FurnitureType, List<GameObject>> playerPool = new Dictionary<FurnitureType, List<GameObject>>();

    // 유니티 에디터에서 스크립트를 오브젝트에 처음 넣을 때 자동으로 호출됩니다.
    // 요청하신 최대 개수를 자동으로 입력해줍니다.


    public static FurnitureController instance; // 다른 스크립트에서 쉽게 부를 수 있도록 싱글톤 처리

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
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

    // 설정된 최대 개수만큼 오브젝트를 미리 생성하여 대기 풀(availablePool)에 넣습니다.
    private void InitializePool()
    {
        foreach (var setup in furnitureSetups)
        {
            availablePool[setup.type] = new List<GameObject>();
            playerPool[setup.type] = new List<GameObject>();

            for (int i = 0; i < setup.maxCount; i++)
            {
                if (setup.prefab == null) 
                {
                    Debug.LogWarning($"{setup.type}의 프리팹이 등록되지 않았습니다.");
                    continue;
                }

                GameObject obj = Instantiate(setup.prefab, transform);
                obj.SetActive(false); // 화면에 보이지 않게 비활성화
                availablePool[setup.type].Add(obj);
            }
        }
    }

    /// <summary>
    /// 플레이어가 가구를 제작할 때 호출하는 함수입니다.
    /// </summary>
    public void CraftFurniture(FurnitureType type, Vector3 spawnPosition)
    {
        // 대기 풀에 해당 가구가 남아있는지 확인 (남아있지 않다면 최대 개수를 초과한 것)
        if (!availablePool.ContainsKey(type) || availablePool[type].Count == 0)
        {
            Debug.LogWarning($"[제작 실패] {type} 가구의 최대 제작 개수에 도달했거나 풀에 존재하지 않습니다.");
            return;
        }

        // 1. 대기 풀에서 가구를 하나 꺼냅니다.
        GameObject furnitureToCraft = availablePool[type][0];

        // 2. 대기 풀에서 삭제하고 플레이어 풀로 이동시킵니다.
        availablePool[type].RemoveAt(0);
        playerPool[type].Add(furnitureToCraft);

        // 3. 위치를 설정하고 활성화합니다.
        furnitureToCraft.transform.position = spawnPosition;
        furnitureToCraft.SetActive(true);

        Debug.Log($"[제작 성공] {type} 가구를 만들었습니다! (현재 플레이어 소유: {playerPool[type].Count}개)");
    }

    /// <summary>
    /// 가구를 철거하거나 파괴해서 다시 대기 풀로 되돌려놓을 때 호출하는 함수입니다.
    /// </summary>
    public void ReturnFurniture(FurnitureType type, GameObject furnitureObj)
    {
        if (playerPool.ContainsKey(type) && playerPool[type].Contains(furnitureObj))
        {
            // 플레이어 풀에서 제거
            playerPool[type].Remove(furnitureObj);

            // 비활성화 후 대기 풀로 다시 이동
            furnitureObj.SetActive(false);
            availablePool[type].Add(furnitureObj);

            Debug.Log($"[회수 성공] {type} 가구를 다시 풀로 되돌렸습니다.");
        }
    }


    // 플레이어가 현재 소유한(제작한) 특정 가구의 개수를 반환합니다.
    public int GetPlayerFurnitureCount(FurnitureType type)
    {
        if (playerPool.ContainsKey(type))
        {
            return playerPool[type].Count;
        }
        return 0;
    }
}