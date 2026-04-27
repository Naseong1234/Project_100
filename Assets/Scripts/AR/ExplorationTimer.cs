using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위한 네임스페이스

public class ExplorationTimer : MonoBehaviour
{
    [Header("타이머 설정")]
    public float timeLimit = 180f; // 3분 = 180초
    private float currentTime;
    private bool isTimerRunning = false;

    [Header("UI 연결 (인스펙터에서 넣으세요)")]
    public TextMeshProUGUI timerText;

    [Header("씬 전환 설정")]
    public string dailySceneName = "DailyScene"; // 돌아갈 일상씬의 정확한 이름을 적어주세요
    private SceneController sceneController;

    void Start()
    {
        // 타이머 초기화 (180초) 및 작동 시작
        currentTime = timeLimit;
        isTimerRunning = true;

        // 씬 내에 있는 SceneController를 알아서 찾아옵니다.
        sceneController = FindFirstObjectByType<SceneController>();

    }

    void Update()
    {
        if (isTimerRunning)
        {
            // Time.deltaTime을 빼서 실시간으로 시간을 줄입니다.
            currentTime -= Time.deltaTime;

            // 시간이 0 이하가 되면 타이머를 멈추고 종료 함수를 부릅니다.
            if (currentTime <= 0)
            {
                currentTime = 0;
                isTimerRunning = false;
                EndExploration();
            }

            // 매 프레임 UI 글자 업데이트
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        // 남은 초(초 단위)를 분(Minutes)과 초(Seconds)로 계산합니다.
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        // 00:00 형태로 문자열을 예쁘게 포맷팅해서 Text에 띄워줍니다.
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void EndExploration()
    {
        Debug.Log("탐험 시간이 모두 끝났습니다! 일상씬으로 복귀합니다.");

        // 작성해주신 SceneController의 함수를 호출하여 저장 + 씬 전환
        if (sceneController != null)
        {
            sceneController.SceneChange(dailySceneName);
        }
    }
}