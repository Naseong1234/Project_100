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

    public static bool isNight = false;
    void Start()
    {
        directionalLight = GetComponent<Light>();

        if (directionalLight == null)
        {
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
    }
}