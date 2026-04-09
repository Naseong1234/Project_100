using System.Collections;
using UnityEngine;

public class SurvivalSystemManager : MonoBehaviour
{
    [Header("Survival Stats")]
    public int day = 1;
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int currentHunger = 100;
    public int currentSanity = 100;

    [Header("Inventory (Resources)")]
    public int wood = 0;
    public int branch = 0;
    public int stone = 0;
    public int mushroom = 0;
    public int flower = 0; // 정신력 상승을 위해 바로 소비할 수도 있지만 일단 인벤토리 처리
    public int meat = 0;
    public int fruit = 0;
    public int edibleGrass = 0;

    private bool isExploring = false;

    // 하루의 시작 (플레이어가 모험 또는 내실을 선택했을 때 호출)
    public void StartNewDay(bool chooseExplore)
    {
        if (isExploring) return; // 이미 진행 중이면 무시

        Debug.Log($"--- Day {day} 시작 ---");

        if (chooseExplore)
        {
            StartCoroutine(ExploreRoutine());
        }
        else
        {
            MaintainBase();
            EndDay(false);
        }
    }

    // 모험 선택 시 (8분 = 480초 동안 진행)
    private IEnumerator ExploreRoutine()
    {
        isExploring = true;
        Debug.Log("모험을 시작합니다. (현실 시간 8분 소요)");

        // 8분(480초) 진행. 테스트를 위해 시간 배속을 하려면 이 부분을 수정하세요.
        float exploreTime = 480f;
        float elapsedTime = 0f;

        // 1분(60초)마다 랜덤 이벤트 발생 (총 8회)
        float eventInterval = 60f;
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

        Debug.Log("모험을 무사히 마치고 돌아왔습니다.");
        isExploring = false;
        EndDay(true); // 모험을 했으므로 true 전달
    }

    // 랜덤 채집/사냥 이벤트
    private void TriggerRandomEncounter()
    {
        int randomEncounter = Random.Range(0, 5); // 0~4
        switch (randomEncounter)
        {
            case 0: // 나무
                wood++;
                branch++;
                fruit++;
                Debug.Log("나무를 벌목했습니다. (나무+1, 나뭇가지+1, 과일+1)");
                break;
            case 1: // 돌
                stone++;
                Debug.Log("돌을 캤습니다. (돌+1)");
                break;
            case 2: // 버섯
                mushroom++;
                Debug.Log("버섯을 채집했습니다. (버섯+1)");
                break;
            case 3: // 꽃
                currentSanity = Mathf.Clamp(currentSanity + 2, 0, 100);
                Debug.Log("꽃을 채집하여 향기를 맡았습니다. (정신력 +2)");
                break;
            case 4: // 동물
                meat++;
                Debug.Log("동물을 사냥했습니다. (고기+1)");
                break;
        }
    }

    // 내실 선택 시
    private void MaintainBase()
    {
        Debug.Log("오늘은 캠프에 남아 내실을 다집니다.");
        // 가구 제작 등의 로직을 여기에 추가할 수 있습니다.
    }

    // 하루 종료 및 스탯 정산
    private void EndDay(bool didExplore)
    {
        // 1. 기본 스탯 감소
        currentHealth -= 5;
        currentHunger -= 5;
        currentSanity -= 5;

        // 2. 모험/내실에 따른 추가 감소
        if (didExplore)
        {
            currentHealth -= 5;
            currentHunger -= 5;
        }

        // 3. 허기에 따른 체력 회복
        if (currentHunger >= 80) currentHealth += 15;
        else if (currentHunger >= 50) currentHealth += 10;
        else currentHealth += 5;

        // 4. 최대 체력 변동 (허기 & 정신력 조건)
        if (currentHunger >= 80 && currentSanity >= 80)
        {
            maxHealth += 1;
        }
        else if (currentHunger >= 50 && currentHunger <= 80 && currentSanity >= 50 && currentSanity <= 80)
        {
            // 50~80 유지 시 변동 없음
            maxHealth += 0;
        }
        else
        {
            // 그 외 모든 경우
            maxHealth -= 1;
        }

        // 최대치를 넘거나 0 이하로 떨어지지 않도록 보정
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentHunger = Mathf.Clamp(currentHunger, 0, 100);
        currentSanity = Mathf.Clamp(currentSanity, 0, 100);

        CheckGameOver();

        day++;
    }

    // 음식 섭취 함수 (UI 버튼 등에서 호출)
    public void EatFood(string foodType)
    {
        switch (foodType)
        {
            case "Mushroom":
                if (mushroom > 0) { mushroom--; currentHunger += 2; }
                break;
            case "EdibleGrass":
                if (edibleGrass > 0) { edibleGrass--; currentHunger += 2; }
                break;
            case "Fruit":
                if (fruit > 0) { fruit--; currentHunger += 5; }
                break;
            case "Meat":
                if (meat > 0) { meat--; currentHunger += 10; }
                break;
        }
        currentHunger = Mathf.Clamp(currentHunger, 0, 100);
    }

    // 생존/사망 판정
    private void CheckGameOver()
    {
        if (currentHealth <= 0)
        {
            Debug.Log("체력이 0이 되어 사망했습니다. Game Over.");
            // 게임 오버 처리 로직
        }
        else if (day >= 100)
        {
            Debug.Log("100일 생존에 성공했습니다! 상금 획득!");
            // 게임 클리어 처리 로직
        }
    }
}
