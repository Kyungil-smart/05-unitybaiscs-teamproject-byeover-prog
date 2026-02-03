using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectBaseTowerBtn : MonoBehaviour
{
    [Header("타워 정보")]
    [SerializeField] int targetID = 1000;
    [SerializeField] int targetLevel = 1;

    [Header("버튼 색상")]
    Color normalColor = Color.white;
    Color selectedColor = new Color(0.65f, 65f, 0.95f, 1f);

    Image image;

    [SerializeField] TextMeshProUGUI descText;

    private void Start()
    {
        image = GetComponent<Image>();
        GameManager.Instance.SelectedBaseID.OnValueChanged += OnSelectedBaseIDChanged;
        GameManager.Instance.SelectedBaseID.Value = targetID;
    }

    private void OnDisable()
    {
        GameManager.Instance.SelectedBaseID.OnValueChanged -= OnSelectedBaseIDChanged;
    }

    void OnSelectedBaseIDChanged(int value)
    {
        // 선택된 기지 ID가 변경될 때 처리할 내용

        if (value == targetID)
        {
            // 강조 표시
            image.color = selectedColor;
            ShowInfo(targetID, targetLevel);
        }
        else
        {
            image.color = normalColor;
        }
    }

    public void OnClick()
    {
        GameManager.Instance.SelectedBaseID.Value = targetID;
        
    }

    public void ShowInfo(int targetID, int targetLevel)
    {
        TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(targetID, targetLevel);

        descText.text = data.desc;
    }
}
