using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject selectPanel;

    // UI 버튼에 연결할 함수
    public void OnClickSlot()
    {
        SceneManager.LoadScene(1); // 게임 씬
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenPanel()
    {
        selectPanel.SetActive(true);
    }
}
