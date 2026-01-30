using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TowerToBuyUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] GameObject TowerToBuyPanel;
    [SerializeField] GameObject[] btnList;
    [SerializeField] TextMeshProUGUI nameText;
    //[SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI attackValueText;
    [SerializeField] TextMeshProUGUI attackRangeText;
    [SerializeField] TextMeshProUGUI attackSpeedText;

    [Header("대상의 정보")]
    [SerializeField] int id = 1101;
    [SerializeField] string name;
    [SerializeField] int level = 1;
    [SerializeField] int maxHP;
    [SerializeField] int attackValue;
    [SerializeField] float attackRange;
    [SerializeField] float attackSpeed;

    [Header("파일 유형 및 경로")]
    [SerializeField] private JsonType jsonType = JsonType.TowerData;
    [SerializeField] string dataFilePath = "Datas/TowerDataExample";  // Json 파일 경로

    public void OnPointerEnter(PointerEventData eventData)
    {
        // UI 활성화 및 정보 설정
        nameText.text = name;
        //hpText.text = maxHP.ToString();
        attackValueText.text = attackValue.ToString();
        attackRangeText.text = attackRange.ToString();
        attackSpeedText.text = attackSpeed.ToString();

        TowerToBuyPanel.SetActive(true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        TowerToBuyPanel.SetActive(false);
    }

    // 호출 받으면 TowerDatas.cs 참고하여 모두 설정해줌
    public void SetupValue(TowerDatas data)
    {
        if (data == null) return;

        id = data.id;
        name = data.name;
        level = data.level;
        maxHP = data.maxHP;
        attackValue = data.attackValue;
        attackRange = data.attackRange;
        attackSpeed = data.attackSpeed;

        Debug.Log($"{name}의 능력치 설정 완료");
    }

    private void Start()
    {
       JsonManager.instanceJsonManger.GetJsonRequest(JsonType.TowerData, dataFilePath, id, level);
    }
}
