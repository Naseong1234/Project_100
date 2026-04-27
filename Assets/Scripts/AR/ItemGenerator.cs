using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    [Header("스폰 설정")]
    // public GameObject Player; <- [삭제] 더 이상 인스펙터에 넣을 필요 없음!

    public GameObject[] gatherablePrefabs;

    // [수정] 스케일을 현실(AR) 크기에 맞게 확 줄였습니다. (테스트용)
    public float minDistance = 0.1f;
    public float maxDistance = 1.0f; // 최대 1미터 반경
    public float objectSpacing = 0.3f; // 간격 30cm

    [Header("레이어 설정 (중요)")]
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
        // ARPlayerController.instance가 없다는 건 아직 캐릭터가 스폰되지 않았다는 뜻이므로 대기
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

            // [핵심] 인스펙터에 넣은 프리팹 위치가 아니라, 실제 스폰된 플레이어의 위치를 가져옴
            Vector3 playerPos = ARPlayerController.instance.transform.position;

            Vector3 rayStartPos = new Vector3(playerPos.x + spawnOffset.x,
                                              playerPos.y + 1f, // 높이도 1미터 위로 살짝 줄임
                                              playerPos.z + spawnOffset.y);

            // [디버그] 에디터 Scene 창에서 레이저가 파란색 선으로 보이게 함 (3초간 유지)
            Debug.DrawRay(rayStartPos, Vector3.down * 3f, Color.blue, 3f);

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

                    // [디버그] 성공적으로 생성된 위치에 빨간색 선 표시
                    Debug.DrawRay(trySpawnPos, Vector3.up * 1f, Color.red, 3f);

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