using UnityEngine;
using UnityEngine.EventSystems;

// 개별 타워 버튼에 붙이는 스크립트 - 각 버튼마다 targetID 할당 // 작성자 : PEY
public class TowerButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("타워 정보")]
    [SerializeField] int targetID;
    [SerializeField] int targetLevel = 1;

    [Header("UI 연결")]
    [SerializeField] TowerToBuyUI towerToBuyUI;

    public int TargetID => targetID;
    public int TargetLevel => targetLevel;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (towerToBuyUI != null)
        {
            RectTransform buttonRect = GetComponent<RectTransform>();
            towerToBuyUI.ShowTowerInfo(targetID, targetLevel, buttonRect);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (towerToBuyUI != null)
        {
            towerToBuyUI.HideTowerInfo();
        }
    }
}
