using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// 작성자 : PEY

/// <summary>
/// UI 요소를 관리하는 컨트롤러입니다.
/// </summary>
public class UIController : MonoBehaviour
{
    public static OP<int> selectedTwID = new();
    public static int selectedTwLevel = 1;

    // 패널
    [SerializeField] GameObject ESCpanel;

    // 텍스트
    [SerializeField] TextMeshProUGUI goldText;

    private void Awake()
    {
        selectedTwID.Value = 0; // 첫 선택 타워
        UpdateGoldText(Player.gold.Value);
    }
    void UpdateGoldText(int value)
    {
        goldText.text = value.ToString();
    }

    void OnEnable()
    {
        Player.gold.OnValueChanged += UpdateGoldText;
    }
    void OnDisable()
    {
        Player.gold.OnValueChanged -= UpdateGoldText;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // esc 패널: 일시정지, 게임재시작, 게임종료
        {
            OpenEscPanel();
        }

        // 디버그용 코드
        if (Input.GetKeyDown(KeyCode.O))
        {
            Player.gold.Value += 100;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Player.gold.Value -= 100;
        }
    }

    public void OpenEscPanel()
    {
        ESCpanel.SetActive(!ESCpanel.activeSelf);
        Time.timeScale = ESCpanel.activeSelf ? 0f : 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // 시간 재개
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬 재로드
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
