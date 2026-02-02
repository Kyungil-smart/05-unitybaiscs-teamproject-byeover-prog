using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 개별 타워 구매 버튼에 붙이는 스크립트 - 각 버튼마다 targetID 할당
public class TwBuyBtnUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("타워 정보")]
    [SerializeField] int targetID = 1101;
    [SerializeField] int targetLevel = 1;
    bool isSelected = false;

    [Header("UI 연결")]
    [SerializeField] TowerToBuyUI towerToBuyUI;

    [Header("버튼 색상")]
    Color normalColor = Color.white;
    Color selectedColor = new Color(0.65f, 1f, 0.65f, 1f);
    Color disabledColor = new Color(0.75f, 0.75f, 0.75f, 0.95f);

    Image image;
    Button btn;

    private void Awake()
    {
        image = GetComponent<Image>();
        btn = GetComponent<Button>();
        UIController.toBuyTwID.Value = -1;
    }

    private void OnEnable()
    {
        UIController.toBuyTwID.OnValueChanged += OnSelectedTowerIDChanged;
        StageManager.gold.OnValueChanged += OnPlayerGoldChanged;
    }
    private void OnDisable()
    {
        UIController.toBuyTwID.OnValueChanged -= OnSelectedTowerIDChanged;
        StageManager.gold.OnValueChanged -= OnPlayerGoldChanged;
    }

    void OnSelectedTowerIDChanged(int value)
    {
        // 선택된 타워 ID가 변경될 때 처리할 내용
        UIController.toBuyTwID.Value = value;

        // 만약 선택된 타워 ID가 이 버튼의 타워 ID와 같다면, 버튼을 강조 표시
        if (value == targetID)
        {
            // 강조 표시 (예: 색상 변경)
            image.color = selectedColor;
        }
        else
        {
            // 기본 상태로 되돌리기
            image.color = normalColor;
            isSelected = false;
        }
    }

    void OnPlayerGoldChanged(int value)
    {
        TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(targetID, targetLevel);

        if (value < data.towerCost) // 골드 부족 판정
        {
            isSelected = false;
            image.color = disabledColor;
            btn.interactable = false;
        }
        else if (!isSelected)
        {
            image.color = normalColor;
            btn.interactable = true;
        }
        else
        {
            image.color = selectedColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        RectTransform buttonRect = GetComponent<RectTransform>();
        towerToBuyUI.ShowInfo(targetID, targetLevel, buttonRect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        towerToBuyUI.HideTowerInfo();
    }

    // 클릭시
    public void OnClickButton()
    {
        UIController.toBuyTwID.Value = targetID;
        //UIController.selectedTowerLevel = targetLevel;
        isSelected = true;
    }
}
