using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    // 이 스크립트는 이번학기 11월 전시회에 출품한 Ashes라는 게임 만들때 만들어봤던 경험이 있어서 그때 경험을 활용해서 만들었습니다.

    public static BGMManager instance = null;

    public AudioClip LoginMusic;
    public AudioClip DailyMusuc;
    public AudioClip ExplorationMusuc;


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
                if (audioSource.clip != LoginMusic)
                {
                    PlayMusic(LoginMusic, 0.3f);
                }
                break;

            case "GuidedScene":
                if (audioSource.clip != LoginMusic)
                {
                    PlayMusic(LoginMusic, 0.3f);
                }
                break;

            case "DailyScene":
                PlayMusic(DailyMusuc, 0.2f);
                break;

            case "ExplorationScene":
                PlayMusic(ExplorationMusuc, 0.2f);
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