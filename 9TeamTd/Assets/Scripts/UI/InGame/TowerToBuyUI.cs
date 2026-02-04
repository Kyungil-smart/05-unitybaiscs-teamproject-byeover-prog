using TMPro;
using UnityEngine;

// 타워 구매 정보를 보여주는 UI // 작성자 : PEY
public class TowerToBuyUI : MonoBehaviour
{
    Canvas canvas;
    [Header("UI")]
    [SerializeField] GameObject TowerToBuyPanel;
    [SerializeField] float offsetYRatio = 0.32f;  // Canvas 높이 대비 비율 

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI descText;
    [SerializeField] TextMeshProUGUI attackValueText;
    [SerializeField] TextMeshProUGUI attackRangeText;
    [SerializeField] TextMeshProUGUI attackSpeedText;
    [SerializeField] TextMeshProUGUI costText;

    [Header("Info")]
    [SerializeField] string towerName;
    [SerializeField] string desc;
    [SerializeField] int attackValue;
    [SerializeField] float attackRange;
    [SerializeField] float attackSpeed;
    [SerializeField] float cost;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    public void ShowInfo(int targetID, int targetLevel, RectTransform buttonRect)
    {
        TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(targetID, targetLevel);
        SetupValue(data);

        nameText.text = towerName;
        descText.text = desc;
        if (attackValue < 0)
        {
            attackValueText.text = "파워: 없음";
            attackSpeedText.text = "속도: 없음";
            attackRangeText.text = "사거리: 없음";
        }
        else
        {
            attackValueText.text = $"파워: {attackValue}";
            attackSpeedText.text = $"속도: {attackSpeed}";
            attackRangeText.text = $"사거리: {attackRange}";
        }
        costText.text = $"<sprite=0>{cost}";

        SetPanelPosition(buttonRect);
        TowerToBuyPanel.SetActive(true);
    }

    public void SetupValue(TowerDatas data)
    {
        towerName = data.name;
        desc = data.desc;
        attackValue = data.attackValue;
        attackRange = data.attackRange;
        attackSpeed = data.attackSpeed;
        cost = data.towerCost;
    }

    // 패널 위치를 버튼 위로 설정
    private void SetPanelPosition(RectTransform buttonRect)
    {
        RectTransform panelRect = TowerToBuyPanel.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 버튼의 월드 좌표를 캔버스 로컬 좌표로 변환
        Vector2 buttonLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, buttonRect.position),
            canvas.worldCamera,
            out buttonLocalPos
        );

        // 캔버스 높이 대비 오프셋 계산
        float offsetY = canvasRect.rect.height * offsetYRatio;

        // 패널 위치 설정 (버튼 위로)
        panelRect.anchoredPosition = new Vector2(buttonLocalPos.x, buttonLocalPos.y + offsetY);
    }

    public void HideTowerInfo()
    {
        TowerToBuyPanel.SetActive(false);
    }
}
