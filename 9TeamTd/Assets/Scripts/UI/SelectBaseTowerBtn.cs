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
    string desc;


    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Start()
    {
        GameManager.Instance.SelectedBaseID.Value = targetID;
        GameManager.Instance.SelectedBaseID.OnValueChanged += OnSelectedBaseIDChanged;
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
            
        }
        else
        {
            image.color = normalColor;
        }
    }

    public void OnClick()
    {
        GameManager.Instance.SelectedBaseID.Value = targetID;
        ShowInfo(targetID, targetLevel);
    }

    public void ShowInfo(int targetID, int targetLevel)
    {
        TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(targetID, targetLevel);

        descText.text = data.desc;
    }
}
