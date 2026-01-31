using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 문형근
// 수정 : 한성우
// 이친구는 어디다가 쓸까요 이렇게 분리하고 하던데 


// 타워 생성, 삭제, 관리 등 기능 구현 예정
// 타워 레벨업, 스킬 적용 등도 여기서 처리할 수 있음
// 건설 가능한 위치 인지, 현재 배치된 타워 목록 관리 


public class TowerManager_Re : MonoBehaviour
{
    //배치 된 타워 저장하는 리스트 (배열로 넣을 예정)
    private List<Tower> _placedTowers = new List<Tower>();

    //타워 프리펩 관련
    public List<GameObject> _towerPrefabs = new List<GameObject>();
    public GameObject _currentTowerPrefab;
    [SerializeField] int _currentTowerIndex = 0;

    // 플레이어 골드
    public int _currentGold = 1000;

    // 타워 가격
    public int _towerCost = 50;


    void Start()
    {

    }

    void Update()
    {
        // 배치할 타워 선택 과정
        SetPlacingTower();




        // 마우스 클릭으로 타워 생성
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[TowerManager] 좌클릭 감지");

            // 마우스 위치에서 Ray 발사
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Ray가 뭔가에 맞았으면 
            if (Physics.Raycast(ray, out hit))
            {
                // 타워를 클릭 했는지 확인
                Tower clickedTower = hit.collider.GetComponent<Tower>();

                // 못찾으면 부모에서 찾기
                if (clickedTower == null)
                {
                    clickedTower = hit.collider.GetComponentInParent<Tower>();
                }
                if (clickedTower != null)
                {
                    Debug.Log("[TowerManager] 타워 클릭 확인 / 선택은 TowerSelector에서 진행");
                }
                else
                {
                    Debug.Log("[TowerManger] Ray가 아무것도 맞지 않음");
                    PlaceTower(hit.point);
                }
            }
        }
    }

    // 타워 선택 함수
    public void SetPlacingTower()
    {
        // 마우스 휠로 생성한 타워 선택
        float mouseScrollInput = Input.GetAxis("Mouse ScrollWheel");

        // 마우스 휠로 인덱스 증감 (음수가 증가로))
        if (mouseScrollInput > 0) _currentTowerIndex -= 1;
        else if (mouseScrollInput < 0) _currentTowerIndex += 1;

        // 예외 처리
        if (_currentTowerIndex < 0) _currentTowerIndex = 0;
        else if (_currentTowerIndex > _towerPrefabs.Count - 1) _currentTowerIndex = _towerPrefabs.Count - 1;

        // 소환할 타워 프리펩 적용
        _currentTowerPrefab = _towerPrefabs[_currentTowerIndex];

        // 마우스 휠 초기화
        mouseScrollInput = 0;
    }


    //타워 생성 함수
    public bool PlaceTower(Vector3 position)
    {
        Debug.Log("타워 배치 함수 시작");

        // 골드 조건 추가
        if (_currentGold < _towerCost)
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }

        if (CanPlaceTower(position) == false)
        {
            return false; // 타워가 있다면 타워가 안생김
        }

        //타워 생성 (Instantiate 복제해서 새오브젝트에 넣기)
        GameObject newTowerObj = Instantiate(_currentTowerPrefab, position, Quaternion.identity);
        Tower newTower = newTowerObj.GetComponent<Tower>();

        //리스트에 추가 (새로운 타워를 리스트에 배열로 추가)
        _placedTowers.Add(newTower);

        // 골드 차감
        _currentGold -= _towerCost;

        Debug.Log("타워 배치 완료! 남은 골드 :" + _currentGold);
        return true; // 배치 성공했으면 완료 
    }

    // 만약에 A B C 조건이라면 타워가 건설 안되야해 
    // A 조건은 ~
    // 리턴하는데 true 여야 함 (조건에 맞으니까?)
    // 부정형이 아니라면 가능한 조건만 만들어서 건설해야함

    // 건설 가능한 위치인지 확인
    public bool CanPlaceTower(Vector3 position)
    {
        // 조건 1 : 해당 위치에 이미 타워가 있는지 체크
        if (CheckTowerExists(position) == false)  // 타워가 없으면 false 반환
        {
            return false;
        }

        //경로를 완전히 막았는지 (다른 분이 구현 예정)
        if (CheckPathBlocked(position) == false)
        {
            return false;
        }

        // 새로운 조건이 있다면  추가하시면됨
        // if (CheckPathBlocked(position) == false)
        // {
        //     return false;
        // }
        return true;
    }
    // 조건 1 : 타워 중복 조건 추가
    private bool CheckTowerExists(Vector3 position)
    {
        // 타워 설치시 거리가 1차이가 나게 우선만들어둠 기능 확인 및 조건을 위해
        // 실제 조건은 타일 (x,y,0 로 될것 같음)
        float checkRadius = 1f;
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Tower")) // 타겟이 타워인 친구들만 체크 에정
            {
                Debug.Log("이미 타워가 있는 위치입니다.!");
                return false;
            }
        }
        return true;
    }
    // 조건 2: 경로 막힘 체크 (팀원이 구현 예정)
    private bool CheckPathBlocked(Vector3 position)
    {
        // TODO: 팀원이 여기에 경로 막힘 로직 구현
        // 경로가 막히면 return false; 줘야함
        return true; // 일단 통과
    }

    public void RemoveTower(Tower tower)
    {
        if (_placedTowers.Contains(tower))
        {
            //50% 환불 (정수 버림)
            int refund = _towerCost / 2;
            _currentGold += refund;

            // 리스트에서 제거
            _placedTowers.Remove(tower);

            // 오브젝트 삭제
            Destroy(tower.gameObject);

            Debug.Log("타워 삭제! 환불 :" + refund + "/남은 골드 :" + _currentGold);
        }
    }
    /// <summary>
    /// 타워 업그레이드
    /// 골드를 소모하고 타워의 능력치를 강화함
    /// </summary>
    /// <param name="tower">업그레이드할 타워</param>
    public void UpgradeTower(Tower tower)
    {
        Debug.Log("[TowerManager] 업그레이드 함수 호출됨");

        // 타워가 null이면 종료
        if (tower == null)
        {
            Debug.LogError("[TowerManager] 업그레이드 실패 - 타워가 null입니다!");
            return;
        }

        Debug.Log($"[TowerManager] 업그레이드 대상: {tower.name}");
        Debug.Log($"[TowerManager] 현재 골드: {_currentGold}G / 필요 골드: {_towerCost}G");

        // 골드 체크
        if (_currentGold < _towerCost)
        {
            Debug.LogWarning("[TowerManager] 업그레이드 실패 - 골드가 부족합니다!");
            Debug.LogWarning($"  필요: {_towerCost}G, 보유: {_currentGold}G, 부족: {_towerCost - _currentGold}G");
            return;
        }

        // 골드 차감
        _currentGold -= _towerCost;
        Debug.Log($"[TowerManager] 골드 차감: -{_towerCost}G (남은 골드: {_currentGold}G)");

        // 업그레이드 전 스탯 저장 (로그용)
        float oldDamage = tower._damage;
        float oldRange = tower._range;

        // 능력치 강화 (임시로 1.5배, 1.1배 적용)
        // TODO: TowerStats, JsonManager와 연동하여 레벨별 스탯 적용
        tower._damage *= 1.5f;
        tower._range *= 1.1f;

        // 업그레이드 결과 출력
        Debug.Log("============================================");
        Debug.Log($"[업그레이드 완료] {tower.name}");
        Debug.Log($"  - 공격력: {oldDamage} → {tower._damage} (+{tower._damage - oldDamage})");
        Debug.Log($"  - 사거리: {oldRange} → {tower._range} (+{tower._range - oldRange})");
        Debug.Log($"  - 남은 골드: {_currentGold}G");
        Debug.Log("============================================");
    }
}
