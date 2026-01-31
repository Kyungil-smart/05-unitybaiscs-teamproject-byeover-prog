using UnityEngine;

// 작성자 : 문형근
// 타워 선택 및 메뉴 처리
// 사용법: 
// 1. 좌클릭으로 타워 선택
// 2. 우클릭으로 메뉴 열기
// 3. 1키: 업그레이드 / 2키: 환불 / ESC: 취소

public class TowerSelector : MonoBehaviour
{
    // TowerManager 참조 - 업그레이드, 환불 기능 호출용
    [SerializeField] private TowerManager _towerManager;

    // 현재 선택된 타워
    private Tower _selectedTower;
    
    // 타워가 선택되었는지 여부
    private bool _hasSelection;
    
    // 메뉴가 열려있는지 여부
    private bool _showingMenu;

    void Start()
    {
        Debug.Log("[TowerSelector] 초기화 시작");
        
        // TowerManager가 Inspector에서 연결 안되어 있으면 자동으로 찾기
        if (_towerManager == null)
        {
            _towerManager = FindObjectOfType<TowerManager>();
            Debug.Log("[TowerSelector] TowerManager 자동 연결 시도");
        }

        // TowerManager를 못 찾으면 에러
        if (_towerManager == null)
        {
            Debug.LogError("[TowerSelector] TowerManager를 찾을 수 없습니다! Inspector에서 연결해주세요.");
        }
        else
        {
            Debug.Log("[TowerSelector] TowerManager 연결 성공!");
        }

        Debug.Log("[TowerSelector] 초기화 완료");
    }

    void Update()
    {
        // 메뉴가 열려있으면 메뉴 입력만 처리
        HandleMenuInput();
        if (_showingMenu) return;

        // ========== 좌클릭: 타워 선택 ==========
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[TowerSelector] 좌클릭 감지");
            
            // 마우스 위치에서 Ray 발사 (화면 좌표 -> 월드 좌표로 광선 쏘기)
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Ray가 뭔가에 맞았는지 확인
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("[TowerSelector] Ray 충돌: " + hit.collider.name);
                
                // 충돌한 오브젝트에서 Tower 컴포넌트 찾기
                Tower clickedTower = hit.collider.GetComponent<Tower>();
                
                // 못 찾으면 부모 오브젝트에서 찾기 (타워의 자식 오브젝트 클릭했을 수도 있으니까)
                if (clickedTower == null)
                {
                    clickedTower = hit.collider.GetComponentInParent<Tower>();
                    Debug.Log("[TowerSelector] 부모에서 Tower 컴포넌트 찾는 중...");
                }

                // 타워를 찾았으면 선택
                if (clickedTower != null)
                {
                    SelectTower(clickedTower);
                }
                else
                {
                    // 타워가 아닌 곳 클릭하면 선택 해제
                    Debug.Log("[TowerSelector] 타워가 아닌 곳 클릭 - 선택 해제");
                    ClearSelection();
                }
            }
            else
            {
                Debug.Log("[TowerSelector] Ray가 아무것도 맞지 않음");
            }
        }

        // ========== 우클릭: 메뉴 열기 ==========
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("[TowerSelector] 우클릭 감지");
            
            // 타워가 선택되어 있으면 메뉴 열기
            if (_hasSelection && _selectedTower != null)
            {
                ShowTowerMenu();
            }
            else
            {
                Debug.Log("[TowerSelector] 선택된 타워가 없어서 메뉴를 열 수 없음");
            }
        }
    }

    // 타워 선택 처리
    private void SelectTower(Tower tower)
    {
        _selectedTower = tower;
        _hasSelection = true;
        
        Debug.Log("============================================");
        Debug.Log("[타워 선택] " + tower.name);
        Debug.Log("  - 공격력: " + tower._damage);
        Debug.Log("  - 사거리: " + tower._range);
        Debug.Log("  - 공격속도: " + tower._attackSpeed);
        Debug.Log("  >> 우클릭으로 메뉴 열기");
        Debug.Log("============================================");
    }

    // 타워 선택 해제
    private void ClearSelection()
    {
        if (_hasSelection)
        {
            Debug.Log("[TowerSelector] 선택 해제됨");
        }
        
        _hasSelection = false;
        _selectedTower = null;
    }

    // 타워 메뉴 표시 (콘솔에 출력)
    private void ShowTowerMenu()
    {
        _showingMenu = true;
        
        Debug.Log("");
        Debug.Log("==============================");
        Debug.Log("        타워 메뉴             ");
        Debug.Log("==============================");
        Debug.Log("  [1] 업그레이드              ");
        Debug.Log("  [2] 환불                    ");
        Debug.Log("  [ESC] 취소                  ");
        Debug.Log("==============================");
        Debug.Log("");
    }

    // 메뉴 입력 처리 (1, 2, ESC 키)
    private void HandleMenuInput()
    {
        // 메뉴가 안 열려있으면 무시
        if (!_showingMenu) return;

        // ========== 1키: 업그레이드 ==========
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            Debug.Log("[TowerSelector] 1키 입력 - 업그레이드 선택");
            
            if (_towerManager != null)
            {
                _towerManager.UpgradeTower(_selectedTower);
            }
            else
            {
                Debug.LogError("[TowerSelector] TowerManager가 없어서 업그레이드 불가!");
            }
            
            CloseMenu();
        }
        // ========== 2키: 환불 ==========
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            Debug.Log("[TowerSelector] 2키 입력 - 환불 선택");
            
            if (_towerManager != null)
            {
                _towerManager.RemoveTower(_selectedTower);
            }
            else
            {
                Debug.LogError("[TowerSelector] TowerManager가 없어서 환불 불가!");
            }
            
            CloseMenu();
        }
        // ========== ESC키: 취소 ==========
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[TowerSelector] ESC키 입력 - 메뉴 닫기");
            CloseMenu();
        }
    }

    // 메뉴 닫기 및 선택 초기화
    private void CloseMenu()
    {
        Debug.Log("[TowerSelector] 메뉴 닫힘");
        
        _showingMenu = false;
        _hasSelection = false;
        _selectedTower = null;
    }
}
