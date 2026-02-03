using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoalTimeUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goalTimeText;

    // 1초 코루틴 캐싱
    WaitForSeconds oneSecondWait = new WaitForSeconds(1f);

    public IEnumerator UpdateGoalTime()
    {
        while (true)
        {
            yield return oneSecondWait;
            SetGoalTimeText();
        }
        
    }

    void SetGoalTimeText()
    {
        float goalTime = StageManager.Instance.stageEndTimeForReset;
        goalTime -= Time.timeSinceLevelLoad;
        int minutes = Mathf.FloorToInt(goalTime / 60f);
        int seconds = Mathf.FloorToInt(goalTime % 60f);
        goalTimeText.text = $"{minutes:00}:{seconds:00}";
    }

    private void Start()
    {
        StartCoroutine(UpdateGoalTime());
    }

    private void LateUpdate()
    {
        
    }
}
