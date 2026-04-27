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
    // 첫 로드 시 '낮'이 되도록 초기값을 false로 설정합니다.
    private static bool isNextNight = false;

    void Start()
    {
        // 이 스크립트가 붙어있는 오브젝트의 Light 컴포넌트를 가져옵니다.
        directionalLight = GetComponent<Light>();

        if (directionalLight == null)
        {
            Debug.LogWarning("DayManager가 있는 오브젝트에 Light 컴포넌트가 없습니다!");
            return;
        }

        // 코드로 색온도(Temperature) 값을 변경하려면 이 옵션을 반드시 켜주어야 합니다.
        directionalLight.useColorTemperature = true;

        // 현재 순서에 맞게 낮 또는 밤을 설정합니다.
        if (isNextNight)
        {
            SetNight();
        }
        else
        {
            SetSun();
        }

        // 다음 번에 일상씬이 로드될 때는 반대 시간대가 되도록 상태를 뒤집어줍니다(Toggle).
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

        // 그림자 강도가 0일 때는 아예 렌더링을 꺼버리는 것이 모바일 최적화(성능)에 좋습니다.
        directionalLight.shadows = LightShadows.None;

        Debug.Log(" [DayManager] 현재 일상씬: 밤 (Night) 모드로 설정되었습니다.");
    }

    private void SetSun()
    {
        // [추가된 부분 1] 낮이 될 때마다 생존 일수(day)를 1 증가시킵니다.
        GameManager.instance.day++;

        Sun_Image.SetActive(true);
        Night_Image.SetActive(false);

        Next_Image.SetActive(false);
        Exploration_Image.SetActive(true);

        // [수정된 부분] 증가된 날짜를 텍스트에 즉시 반영합니다.
        Day_Text.text = $"Day - {GameManager.instance.day.ToString()}";

        directionalLight.colorTemperature = 7000f;
        directionalLight.intensity = 2.5f;
        directionalLight.shadowStrength = 0.5f;

        // 낮이 되었으니 그림자를 다시 부드럽게 켜줍니다.
        directionalLight.shadows = LightShadows.Soft;

        // [추가된 부분 2] 날짜가 올랐으므로 즉시 게임 데이터를 저장하여 기기에 반영합니다.
        if (DataSaveManager.instance != null)
        {
            DataSaveManager.instance.SaveGameData();
        }

        Debug.Log($" [DayManager] 현재 일상씬: 낮 (Day) 모드로 설정되었습니다. 현재 생존 일수: {GameManager.instance.day}일");
    }
}