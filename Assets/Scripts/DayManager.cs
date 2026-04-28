using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayManager : MonoBehaviour
{
    private Light directionalLight;
    public GameObject Sun_Image;
    public GameObject Night_Image;
    public GameObject Next_Image;
    public GameObject Exploration_Image;

    public TextMeshProUGUI Day_Text;

    // static 변수를 사용하여 씬이 넘어갔다 와도 이전 상태를 기억하도록 합니다.
    private static bool isNextNight = false;

    // [추가된 핵심 변수] 게임 접속 후 '최초'로 일상씬에 들어왔는지 판별합니다.
    public static bool isFirstLogin = true;

    void Start()
    {
        directionalLight = GetComponent<Light>();

        if (directionalLight == null)
        {
            Debug.LogWarning("DayManager가 있는 오브젝트에 Light 컴포넌트가 없습니다!");
            return;
        }

        directionalLight.useColorTemperature = true;

        if (isNextNight)
        {
            SetNight();
        }
        else
        {
            SetSun();
        }

        isNextNight = !isNextNight;
    }

    private void SetNight()
    {
        Sun_Image.SetActive(false);
        Night_Image.SetActive(true);

        Next_Image.SetActive(true);
        Exploration_Image.SetActive(false);

        Day_Text.text = $"Day - {GameManager.instance.day.ToString()}";
        directionalLight.colorTemperature = 20000f;
        directionalLight.intensity = 1f;
        directionalLight.shadowStrength = 0f;

        directionalLight.shadows = LightShadows.None;

        Debug.Log(" [DayManager] 현재 일상씬: 밤 (Night) 모드로 설정되었습니다.");
    }

    private void SetSun()
    {
        // [수정된 로직] 최초 로그인 시에는 날짜 증가 로직을 건너뜁니다.
        if (isFirstLogin)
        {
            Debug.Log(" [DayManager] 최초 로그인 감지: 날짜를 증가시키지 않습니다.");
            isFirstLogin = false; // 다음번 낮이 올 때는 정상적으로 날짜가 오르도록 false로 바꿔줍니다.
        }
        else
        {
            // 탐험을 마치고 돌아왔거나, 밤에서 낮으로 넘어갈 때만 날짜를 올립니다.
            GameManager.instance.day++;

            // 날짜가 올랐을 때만 저장하도록 이사 왔습니다.
            if (DataSaveManager.instance != null)
            {
                DataSaveManager.instance.SaveGameData();
            }
        }

        Sun_Image.SetActive(true);
        Night_Image.SetActive(false);

        Next_Image.SetActive(false);
        Exploration_Image.SetActive(true);

        directionalLight.colorTemperature = 7000f;
        directionalLight.intensity = 2.5f;
        directionalLight.shadowStrength = 0.5f;

        directionalLight.shadows = LightShadows.Soft;

        Debug.Log($" [DayManager] 현재 일상씬: 낮 (Day) 모드로 설정되었습니다. 현재 생존 일수: {GameManager.instance.day}일");
    }
}