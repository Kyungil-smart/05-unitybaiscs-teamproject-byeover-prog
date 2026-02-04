using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class SceneLoader : MonoBehaviour
{
    // UI 버튼에 연결할 함수
    public void OnClickStartStage(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex); // 게임 씬
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
