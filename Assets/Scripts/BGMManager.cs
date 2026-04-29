using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{

    public static BGMManager instance = null;

    public AudioClip Login_BGM;
    public AudioClip Daily_Sun_BGM;
    public AudioClip Daily_Night_BGM;
    public AudioClip Exploration_BGM;

    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "LoginScene":
                if (audioSource.clip != Login_BGM)
                {
                    PlayMusic(Login_BGM, 0.3f);
                }
                break;

            case "DailyScene": 
                // DayManager의 isNight 상태를 확인하여 BGM 분기 처리
                if (DayManager.isNight)
                {
                    PlayMusic(Daily_Night_BGM, 0.2f); // 밤일 경우
                }
                else
                {
                    PlayMusic(Daily_Sun_BGM, 0.2f);   // 낮일 경우
                }
                break;

            case "ExplorationScene":
                PlayMusic(Exploration_BGM, 0.2f);
                break;

            default:
                audioSource.Stop();
                break;
        }
    }

    void PlayMusic(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}