using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 타워 업그레이드/판매 정보 패널 UI
public class TowerToUpSellUI : MonoBehaviour
{
    Canvas canvas;
    Camera mainCamera;

    [Header("UI")]
    [SerializeField] GameObject TowerToUpSellPanel;
    [SerializeField] float offsetYRatio = 0.32f;  // Canvas 높이 대비 비율

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI costText;

    [Header("대상의 정보")]
    string towerName;
    public int cost;

    Transform targetTower;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        mainCamera = Camera.main;
    }

    public void ShowInfo(int targetID, int targetLevel, Transform towerTransform)
    {
        TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(targetID, targetLevel);
        SetupValue(data);

        nameText.text = towerName;
        costText.text = $"<sprite=0>{cost}";

        targetTower = towerTransform;
        UpdatePanelPosition();
        TowerToUpSellPanel.SetActive(true);
    }

    public void SetupValue(TowerDatas data)
    {
        towerName = data.name;
        cost = data.towerCost;
    }

    // 매 프레임 패널 위치 갱신 (카메라 이동 후)
    void LateUpdate()
    {
        if (targetTower != null && TowerToUpSellPanel.activeSelf)
        {
            UpdatePanelPosition();
        }
    }

    void UpdatePanelPosition()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetTower.position);
        SetPanelPositionFromScreen(screenPos);
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
        targetTower = null;
        TowerToUpSellPanel.SetActive(false);
    }
}
