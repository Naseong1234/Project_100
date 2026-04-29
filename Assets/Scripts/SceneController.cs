using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void SceneChange(string sceneName)
    {
        // 다른 씬으로 넘어가기 직전, 현재 씬에서의 상태를 기기에 저장
        if (DataSaveManager.instance != null)
        {
            DataSaveManager.instance.SaveGameData();
        }

        // 이동한 새 씬의 DataSaveManager가 Start()에서 알아서 데이터를 불러올거임
        SceneManager.LoadScene(sceneName);
    }
    public void LoginSceneChange(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void CallEndDay()
    {
        // GameManage가 존재한다면 EndDay 실행
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