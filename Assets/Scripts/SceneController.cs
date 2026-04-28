using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void SceneChange(string sceneName)
    {
        // 1. 다른 씬으로 넘어가기 직전, 현재 씬에서의 상태를 기기에 저장합니다.
        if (DataSaveManager.instance != null)
        {
            DataSaveManager.instance.SaveGameData();
        }

        // 2. 씬을 이동합니다.
        // 이동한 새 씬의 DataSaveManager가 Start()에서 알아서 데이터를 불러올 것입니다.
        SceneManager.LoadScene(sceneName);
    }
    public void LoginSceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void CallEndDay()
    {
        // 진짜 GameManager(살아남은 녀석)가 존재한다면 EndDay 실행!
        if (GameManager.instance != null)
        {
            GameManager.instance.EndDay();
        }
        else
        {
            Debug.LogWarning("GameManager를 찾을 수 없습니다!");
        }
    }
}