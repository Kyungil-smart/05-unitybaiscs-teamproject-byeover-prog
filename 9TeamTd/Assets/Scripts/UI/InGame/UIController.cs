using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : PEY

/// <summary>
/// UI 요소를 관리하는 컨트롤러입니다.
/// </summary>
public class UIController : MonoBehaviour
{
    // 패널
    [SerializeField] GameObject ESCpanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // esc 패널: 일시정지, 게임재시작, 게임종료
        {
            ESCpanel.SetActive(!ESCpanel.activeSelf);
            Time.timeScale = ESCpanel.activeSelf ? 0f : 1f;
        }
    }
}
