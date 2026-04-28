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
    public static bool isNight = false;
    void Start()
    {
        directionalLight = GetComponent<Light>();

        if (directionalLight == null)
        {
            Debug.LogWarning("DayManager가 있는 오브젝트에 Light 컴포넌트가 없습니다!");
            return;
        }

        directionalLight.useColorTemperature = true;

        if (isNight)
        {
            SetNight();
        }
        else
        {
            SetSun();
        }

        isNight = !isNight;
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