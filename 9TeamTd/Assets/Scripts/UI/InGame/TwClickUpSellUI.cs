using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

// 인게임 타워를 클릭했을때 업그레이드/판매 패널을 표시하는 UI
public class TwClickUpSellUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] TowerToUpSellUI towerToUpSellUI;
    Camera mainCamera;

    // 버튼
    [SerializeField] GameObject upgradeButton;
    [SerializeField] GameObject sellButton;

    // 레벨 프레임
    [SerializeField] GameObject Frame1;
    [SerializeField] GameObject Frame2;
    [SerializeField] GameObject Frame3;
    [SerializeField] GameObject Frame4;
    [SerializeField] GameObject Frame5;

    static TowerStats selectedTower;
    static Vector3 worldPoint;
    static Cell worldCell;

    void Awake()
    {
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
        // UI를 클릭했으면 무시하는 유니티 꿀기능
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            TowerStats tower = hit.collider.GetComponent<TowerStats>()
                       ?? hit.collider.GetComponentInParent<TowerStats>();

            if (tower != null)
            {
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                ground.Raycast(ray, out float enter);

                worldPoint = ray.GetPoint(enter);
                worldCell = GridSystem.Instance.WorldToCell(worldPoint); // 그리드 시스템 연동

                selectedTower = tower;

                if (tower.level + 1 > 5 || tower.id == 1100 || tower.id == 1101)
                {
                    upgradeButton.SetActive(false);
                    towerToUpSellUI.ShowInfo(tower.id, tower.level, tower.transform);
                }
                else
                {
                    upgradeButton.SetActive(true);
                    towerToUpSellUI.ShowInfo(tower.id, tower.level + 1, tower.transform);
                }
                // 레벨 프레임 변경
                switch (selectedTower.level)
                {
                    case 1:
                        Frame1.SetActive(true);
                        break;
                    case 2:
                        Frame2.SetActive(true);
                        break;
                    case 3:
                        Frame3.SetActive(true);
                        break;
                    case 4:
                        Frame4.SetActive(true);
                        break;
                    case 5:
                        Frame5.SetActive(true);
                        break;
                }
                return;
            }
        }

        HidePanel();
    }

    void HidePanel()
    {
        selectedTower = null;
        towerToUpSellUI.HidePanel();
        Frame1.SetActive(false);
        Frame2.SetActive(false);
        Frame3.SetActive(false);
        Frame4.SetActive(false);
        Frame5.SetActive(false);
    }

    // 업그레이드 버튼 클릭 시
    public void OnUpgradeClick()
    {
        if (selectedTower == null) return;
        
        if (Player.gold.Value < towerToUpSellUI.cost)
        {
#if UNITY_EDITOR
            Debug.Log("골드 부족으로 업그레이드 불가");
#endif
            return;
        }
        Player.gold.Value -= towerToUpSellUI.cost;

        selectedTower.LevelUp();

        // 레벨 프레임 변경
        switch (selectedTower.level)
        {
            case 2:
                Frame1.SetActive(false);
                Frame2.SetActive(true);
                break;
            case 3:
                Frame2.SetActive(false);
                Frame3.SetActive(true);
                break;
            case 4:
                Frame3.SetActive(false);
                Frame4.SetActive(true);
                break;
            case 5:
                Frame4.SetActive(false);
                Frame5.SetActive(true);
                upgradeButton.SetActive(false);
                break;
        }
        towerToUpSellUI.ShowInfo(selectedTower.id, selectedTower.level, selectedTower.transform);
    }

    // 판매 버튼 클릭 시
    public void OnSellClick()
    {
        if (selectedTower == null) return;
        selectedTower.gameObject.SetActive(false);
        Player.gold.Value += selectedTower.towerCost / 2;

        GridSystem.Instance.SetCellState(worldCell, GridSystem.CellState.Empty);

        HidePanel();
    }
}
