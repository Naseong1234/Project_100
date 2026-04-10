using System.Collections.Generic;
using UnityEngine;

// 1. 가구의 종류를 명확하게 관리하기 위해 Enum(열거형)을 사용합니다.
public enum FurnitureType
{
    Barrel_Small, Barrel_Big, Barrel_drink,
    Tent_Small, Tent_Big, Tent_Rest,
    Chair1, Chair2, Boxs,
    Table_Round, Table_Small, Table_Big
}

// 2. 인스펙터에서 각 가구의 프리팹과 최대 개수를 설정할 수 있는 데이터 클래스입니다.
[System.Serializable]
public class FurniturePool
{
    public FurnitureType type;
    public GameObject prefab;
    public int maxCount;

    // [HideInInspector]를 붙여서 유니티 에디터 창에는 안 보이게 하고 내부적으로만 리스트를 관리합니다.
    [HideInInspector] public List<GameObject> availableList = new List<GameObject>(); // 미사용 (제작 대기중인 풀)
    [HideInInspector] public List<GameObject> playerList = new List<GameObject>();    // 사용중 (플레이어가 제작한 풀)
}

public class FurnitureController : MonoBehaviour
{
    public static FurnitureController instance; // 다른 스크립트에서 쉽게 부를 수 있도록 싱글톤 처리

    [Header("가구 풀 설정 (인스펙터에서 설정하세요)")]
    public List<FurniturePool> furniturePools;

    // 가구 종류(타입)로 빠르게 풀을 찾기 위한 딕셔너리
    private Dictionary<FurnitureType, FurniturePool> poolDictionary;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        InitializePools();
    }

    // 게임 시작 시 모든 가구를 최대 개수만큼 미리 생성해둡니다.
    private void InitializePools()
    {
        poolDictionary = new Dictionary<FurnitureType, FurniturePool>();

        // 하이어라키(Hierarchy) 창이 지저분해지지 않게 가구들을 담아둘 빈 부모 객체를 하나 만듭니다.
        GameObject poolParent = new GameObject("Furniture_Pool_Manager");
        poolParent.transform.SetParent(this.transform);

        foreach (var pool in furniturePools)
        {
            poolDictionary.Add(pool.type, pool);

            for (int i = 0; i < pool.maxCount; i++)
            {
                if (pool.prefab != null)
                {
                    // 오브젝트 생성 후 부모 지정
                    GameObject obj = Instantiate(pool.prefab, poolParent.transform);
                    obj.SetActive(false); // 처음에는 안 보이게 비활성화

                    // 기존에 만든 리스트 pool(availableList)에 담아둡니다.
                    pool.availableList.Add(obj);
                }
                else
                {
                    Debug.LogWarning($"{pool.type}의 프리팹이 비어있습니다. 인스펙터를 확인해주세요!");
                }
            }
        }
    }

    /// <summary>
    /// 플레이어가 가구를 제작할 때 부르는 함수입니다.
    /// </summary>
    /// <param name="type">만들고자 하는 가구 타입</param>
    /// <param name="spawnPosition">가구가 설치될 위치</param>
    public GameObject CraftFurniture(FurnitureType type, Vector3 spawnPosition)
    {
        if (!poolDictionary.ContainsKey(type)) return null;

        FurniturePool pool = poolDictionary[type];

        // 1. 기존 풀(availableList)에 남은 가구가 있는지 확인합니다.
        if (pool.availableList.Count > 0)
        {
            // 2. 대기 풀에서 하나를 꺼냅니다.
            GameObject furnitureToCraft = pool.availableList[0];
            pool.availableList.RemoveAt(0);

            // 3. 플레이어 리스트 풀로 이동시킵니다.
            pool.playerList.Add(furnitureToCraft);

            // 4. 위치를 설정하고 눈에 보이게 활성화합니다.
            furnitureToCraft.transform.position = spawnPosition;
            furnitureToCraft.SetActive(true);

            Debug.Log($"[{type}] 제작 성공! (남은 개수: {pool.availableList.Count} / {pool.maxCount})");
            return furnitureToCraft;
        }
        else
        {
            // 최대 개수에 도달해서 대기 풀이 비어있는 경우
            Debug.LogWarning($"[{type}] 제작 실패: 최대 설치 개수({pool.maxCount}개)에 도달했습니다!");
            return null;
        }
    }

    /// <summary>
    /// 플레이어가 가구를 부수거나 회수할 때 다시 대기 풀로 돌려보내는 함수입니다.
    /// </summary>
    public void ReturnFurniture(FurnitureType type, GameObject furnitureObj)
    {
        if (!poolDictionary.ContainsKey(type)) return;

        FurniturePool pool = poolDictionary[type];

        if (pool.playerList.Contains(furnitureObj))
        {
            // 플레이어 풀에서 제거하고 다시 대기 풀로 이동
            pool.playerList.Remove(furnitureObj);
            pool.availableList.Add(furnitureObj);

            // 다시 화면에서 숨김
            furnitureObj.SetActive(false);
            Debug.Log($"[{type}] 회수 완료. 다시 제작할 수 있습니다.");
        }
    }
}