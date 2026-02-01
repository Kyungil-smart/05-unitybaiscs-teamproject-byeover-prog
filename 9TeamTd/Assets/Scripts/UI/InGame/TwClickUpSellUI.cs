using UnityEngine;
using TMPro;
using UnityEngine.UI;

// 인게임 타워를 클릭했을때 업그레이드/판매 패널을 표시하는 UI
public class TwClickUpSellUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] TowerToUpSellUI towerToUpSellUI;
    [SerializeField] Camera mainCamera;

    // 버튼
    [SerializeField] GameObject upgradeButton;
    [SerializeField] GameObject sellButton;

    TowerStats selectedTower;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectTower();
        }
    }

    void TrySelectTower()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            TowerStats tower = hit.collider.GetComponent<TowerStats>()
                       ?? hit.collider.GetComponentInParent<TowerStats>();

            if (tower != null)
            {
                selectedTower = tower;
                ShowUpSellPanel(tower);

                if (tower.level + 1 > 5 || tower.id == 1100 || tower.id == 1101)
                {
                    upgradeButton.SetActive(false);
#if UNITY_EDITOR
                    Debug.Log("아아");
#endif
                    return;
                }

                towerToUpSellUI.ShowInfo(tower.id, tower.level + 1,
                    mainCamera.WorldToScreenPoint(tower.transform.position));
                return;
            }
        }

        // 타워 외 클릭 시 패널 숨김
        HidePanel();
    }

    void ShowUpSellPanel(TowerStats tower)
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(tower.transform.position);
        towerToUpSellUI.ShowInfo(tower.id, tower.level, screenPos);
    }

    void HidePanel()
    {
        selectedTower = null;
        towerToUpSellUI.HidePanel();
    }

    // 업그레이드 버튼 클릭 시 (Button OnClick에 연결)
    public void OnUpgradeClick()
    {
        if (selectedTower == null) return;
        // TODO: UpgradeTower(selectedTower);
        HidePanel();
    }

    // 판매 버튼 클릭 시 (Button OnClick에 연결)
    public void OnSellClick()
    {
        if (selectedTower == null) return;
        // TODO: RemoveTower(selectedTower);
        HidePanel();
    }
}
