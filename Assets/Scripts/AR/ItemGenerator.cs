using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    [Header("스폰 설정")]

    public GameObject[] gatherablePrefabs;

    public float minDistance = 0.1f;
    public float maxDistance = 1.0f; // 최대 1미터
    public float objectSpacing = 0.3f; // 간격 30cm

    public LayerMask groundLayer;
    public LayerMask itemLayer;

    [Header("시간 및 수량 제어")]
    public float minTime = 2.0f;
    public float maxTime = 5.0f;
    public int maxGatherables = 15;

    private float currentTime = 0.0f;
    private float nextSpawnTime;
    private int currentGatherableCount = 0;


    void Start()
    {
        nextSpawnTime = Random.Range(minTime, maxTime);
    }

    void Update()
    {
        CreateItem();
    }

    void CreateItem()
    {
        if (ARPlayerController.instance == null) return;

        if (currentGatherableCount < maxGatherables)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= nextSpawnTime)
            {
                SpawnGatherable();

                currentTime = 0f;
                nextSpawnTime = Random.Range(minTime, maxTime);
            }
        }
    }

    void SpawnGatherable()
    {
        for (int i = 0; i < 20; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minDistance, maxDistance);
            Vector2 spawnOffset = randomDir * randomDistance;

            // 인스펙터에 넣은 프리팹 위치가 아니라, 실제 스폰된 플레이어의 위치를 가져옴
            Vector3 playerPos = ARPlayerController.instance.transform.position;

            Vector3 rayStartPos = new Vector3(playerPos.x + spawnOffset.x,playerPos.y + 1f, playerPos.z + spawnOffset.y);

            RaycastHit hit;
            if (Physics.Raycast(rayStartPos, Vector3.down, out hit, 3f, groundLayer))
            {
                Vector3 trySpawnPos = hit.point;

                bool isOverlapping = Physics.CheckSphere(trySpawnPos, objectSpacing / 2f, itemLayer);

                if (!isOverlapping)
                {
                    int randomIndex = Random.Range(0, gatherablePrefabs.Length);
                    Instantiate(gatherablePrefabs[randomIndex], trySpawnPos, Quaternion.identity);

                    currentGatherableCount++;

                    break;
                }
            }
        }
    }

    public void DecreaseGatherableCount()
    {
        currentGatherableCount--;
        if (currentGatherableCount < 0) currentGatherableCount = 0;
    }

    public void OnButtonEvent()
    {
        ARPlayerController.instance.OnGatheringButtonClicked();
    }
}