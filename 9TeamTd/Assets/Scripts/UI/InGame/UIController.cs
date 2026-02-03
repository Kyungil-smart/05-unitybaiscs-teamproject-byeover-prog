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
    public static OP<int> toBuyTwID = new();
    Tower baseTower;

    // 패널
    [SerializeField] GameObject ESCpanel;
    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject defeatPanel;

    // 텍스트
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI hpText;

    private void Awake()
    {
        UpdateGoldText(StageManager.gold.Value);
    }
    void UpdateGoldText(int value)
    {
        goldText.text = value.ToString();
    }

    void OnEnable()
    {
        StageManager.gold.OnValueChanged += UpdateGoldText;
        StageManager.Instance.StageClear += ShowVictoryPanel;
        StageManager.Instance.StageDefeat += ShowDefeatPanel;
    }
    void OnDisable()
    {
        StageManager.gold.OnValueChanged -= UpdateGoldText;
        StageManager.Instance.StageClear -= ShowVictoryPanel;
        StageManager.Instance.StageDefeat -= ShowDefeatPanel;
    }

    void ShowVictoryPanel()
    {
        victoryPanel.SetActive(true);
    }
    void ShowDefeatPanel()
    {
        defeatPanel.SetActive(true);
    }

    private void Start()
    {

        StartCoroutine(FindBaseTower());

    }

    IEnumerator FindBaseTower()
    {
        while (baseTower == null)
        {
            baseTower = FindFirstObjectByType<Tower>();

            yield return null;
        }
#if UNITY_EDITOR
        Debug.Log("타워 발견" + baseTower);
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // esc 패널: 일시정지, 게임재시작, 게임종료
        {
            OpenEscPanel();
        }
        hpText.text = $"<sprite=0>{baseTower._currentHP}";

        // 디버그용 코드
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            StageManager.gold.Value += 1000;
#if UNITY_EDITOR
            Debug.Log("돈무한 치트 사용!!!");
#endif
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            StageManager.Instance.OnDefeat();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StageManager.Instance.stageEndTimeForReset = 0;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            baseTower._currentHP += 100;
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
        GameManager.Instance.ChangeBGM();
    }
}
