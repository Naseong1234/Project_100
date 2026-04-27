using UnityEngine;

public class GatherableGenerator : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject Player;
    // 인스펙터에서 생성할 물체들을 배열에 넣어주세요 (나무, 동물, 버섯, 꽃, 덤불 등)
    public GameObject[] gatherablePrefabs;

    public float minDistance = 1f; // 플레이어로부터 최소 거리
    public float maxDistance = 6f; // 플레이어로부터 최대 거리
    public float objectSpacing = 1f; // 생성 물체 간의 최소 간격 (겹침 방지)

    [Header("레이어 설정 (중요)")]
    public LayerMask groundLayer; // AR Plane을 인식할 레이어 (Ground)
    public LayerMask itemLayer;   // 생성된 물체들이 겹치는지 확인할 레이어

    [Header("시간 및 수량 제어")]
    public float minTime = 2.0f;
    public float maxTime = 5.0f;
    public int maxGatherables = 15; // 필드에 존재할 수 있는 최대 채집물 수

    private float currentTime = 0.0f;
    private float nextSpawnTime;
    private int currentGatherableCount = 0; // 현재 생성된 갯수 관리

    void Start()
    {
        // 첫 생성 주기 설정
        nextSpawnTime = Random.Range(minTime, maxTime);
    }

    void Update()
    {
        CreateItem();
    }

    void CreateItem()
    {
        // 최대 채집물 갯수를 넘지 않았을 때만 생성 시도
        if (currentGatherableCount < maxGatherables)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= nextSpawnTime)
            {
                SpawnGatherable();

                // 생성 주기 초기화
                currentTime = 0f;
                nextSpawnTime = Random.Range(minTime, maxTime);
            }
        }
    }

    void SpawnGatherable()
    {
        // 랜덤 생성을 시도하지만, 조건에 안맞을 수 있으므로 최대 20번만 자리를 찾아봄 (무한루프 방지)
        for (int i = 0; i < 20; i++)
        {
            // 1. 플레이어 기준으로 1~6 거리의 랜덤한 X, Z 좌표(방향) 계산
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minDistance, maxDistance);
            Vector2 spawnOffset = randomDir * randomDistance;

            // 플레이어 머리 위쪽(Y: 2f)에서 시작해서 아래로 탐색할 시작점
            Vector3 rayStartPos = new Vector3(Player.transform.position.x + spawnOffset.x,
                                              Player.transform.position.y + 2f,
                                              Player.transform.position.z + spawnOffset.y);

            // 2. 하늘에서 아래로(Vector3.down) 레이캐스트를 쏴서 AR Plane(Ground)이 있는지 확인
            RaycastHit hit;
            if (Physics.Raycast(rayStartPos, Vector3.down, out hit, 5f, groundLayer))
            {
                // 레이저가 닿은 바닥의 정확한 위치
                Vector3 trySpawnPos = hit.point;

                // 3. 해당 위치에 다른 채집물이 있는지 확인 (겹침 방지 조건)
                // CheckSphere는 지정된 반경 내에 itemLayer를 가진 콜라이더가 있으면 true를 반환함
                bool isOverlapping = Physics.CheckSphere(trySpawnPos, objectSpacing / 2f, itemLayer);

                if (!isOverlapping)
                {
                    // 조건 통과! 랜덤한 채집물 하나를 골라서 스폰
                    int randomIndex = Random.Range(0, gatherablePrefabs.Length);
                    GameObject newObj = Instantiate(gatherablePrefabs[randomIndex], trySpawnPos, Quaternion.identity);

                    currentGatherableCount++;
                    break; // 성공적으로 생성했으니 탐색 루프(for문) 즉시 종료
                }
            }
        }
    }

    // 채집물을 플레이어가 캐거나 파괴했을 때, 카운트를 줄여주는 함수 (외부에서 호출 용도)
    public void DecreaseGatherableCount()
    {
        currentGatherableCount--;
        if (currentGatherableCount < 0) currentGatherableCount = 0;
    }
}