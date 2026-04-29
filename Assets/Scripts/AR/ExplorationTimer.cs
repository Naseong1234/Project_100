using UnityEngine;
using TMPro;

public class ExplorationTimer : MonoBehaviour
{
    [Header("타이머 설정")]
    public float timeLimit = 180f; // 3분 = 180초
    private float currentTime;
    private bool isTimerRunning = false;

    [Header("UI 연결")]
    public TextMeshProUGUI timerText;

    [Header("씬 전환 설정")]
    public string dailySceneName = "DailyScene"; 
    private SceneController sceneController;

    void Start()
    {
        // 타이머 초기화 및 작동 시작
        currentTime = timeLimit;
        isTimerRunning = true;

        // 씬 내에 있는 SceneController를 알아서 찾아오기
        sceneController = FindFirstObjectByType<SceneController>();

    }

    void Update()
    {
        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;

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
        // 남은 초를 분과 초로 계산
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        // 00:00 형태로 문자열을 예쁘게 포맷팅해서 Text에 띄워주기
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void EndExploration()
    {
        if (sceneController != null)
        {
            sceneController.SceneChange(dailySceneName);
        }
    }
}