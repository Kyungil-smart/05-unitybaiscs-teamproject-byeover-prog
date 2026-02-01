using UnityEngine;
using TMPro;

// 타워 업그레이드/판매 정보 패널 UI
public class TowerToUpSellUI : MonoBehaviour
{
    Canvas canvas;
    [Header("UI")]
    [SerializeField] GameObject TowerToUpSellPanel;
    [SerializeField] float offsetYRatio = 0.32f;  // Canvas 높이 대비 비율

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI costText;

    [Header("대상의 정보")]
    string towerName;
    float cost;

    //Ray ray; RaycastHit hit;
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    public void ShowInfo(int targetID, int targetLevel, RectTransform clickedTower)
    {
        TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(targetID, targetLevel);
        SetupValue(data);

        nameText.text = towerName;
        costText.text = $"<sprite=0>{cost}";

        SetPanelPosition(clickedTower);
        TowerToUpSellPanel.SetActive(true);
    }
    public void ShowInfo(int targetID, int targetLevel, Vector3 screenPos)
    {
        TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(targetID, targetLevel);
        SetupValue(data);

        nameText.text = towerName;
        costText.text = $"<sprite=0>{cost}";

        SetPanelPositionFromScreen(screenPos);
        TowerToUpSellPanel.SetActive(true);
    }
    public void SetupValue(TowerDatas data)
    {
        towerName = data.name;
        cost = data.towerCost;
    }
    // 패널 위치를 버튼 위로 설정
    private void SetPanelPosition(RectTransform clickedTower)
    {
        RectTransform panelRect = TowerToUpSellPanel.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // 버튼의 월드 좌표를 캔버스 로컬 좌표로 변환
        Vector2 clickedTowerLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, clickedTower.position),
            canvas.worldCamera,
            out clickedTowerLocalPos
        );

        // 캔버스 높이 대비 오프셋 계산
        float offsetY = canvasRect.rect.height * offsetYRatio;

        // 패널 위치 설정 (버튼 위로)
        panelRect.anchoredPosition = new Vector2(clickedTowerLocalPos.x, clickedTowerLocalPos.y + offsetY);
    }

    private void SetPanelPositionFromScreen(Vector3 screenPos)
    {
        RectTransform panelRect = TowerToUpSellPanel.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, canvas.worldCamera, out Vector2 localPos
        );

        float offsetY = canvasRect.rect.height * offsetYRatio;
        panelRect.anchoredPosition = new Vector2(localPos.x, localPos.y + offsetY);
    }


    public void HidePanel()
    {
        TowerToUpSellPanel.SetActive(false);
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //        if (Physics.Raycast(ray, out hit))
    //        {
    //            if (hit.collider.CompareTag("Tower"))
    //            {
    //                TowerToUpSellPanel.SetActive(true);
    //            }
    //            else
    //            {
    //                TowerToUpSellPanel.SetActive(false);
    //            }
    //        }
    //        else
    //        {
    //            TowerToUpSellPanel.SetActive(false);
    //        }
    //    }
    //}
}
