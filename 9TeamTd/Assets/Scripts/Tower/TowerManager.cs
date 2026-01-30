using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 문형근
// 이친구는 어디다가 쓸까요 이렇게 분리하고 하던데 


// 타워 생성, 삭제, 관리 등 기능 구현 예정
// 타워 레벨업, 스킬 적용 등도 여기서 처리할 수 있음
// 건설 가능한 위치 인지, 현재 배치된 타워 목록 관리 


public class TowerManager : MonoBehaviour
{
    //배치 된 타워 저장하는 리스트 (배열로 넣을 예정)
    private List<Tower> _placedTowers = new List<Tower>();
    
    //타워 프리팹 
    public GameObject _towerPrefab;
    
    // 플레이어 골드
    public int _gold = 100;

    // 타워 가격
    public int _towerCost = 50;
    

    void Start()
    {
        
    }

    void Update()
    {
        // 마우스 클릭으로 타우 생성
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 위치에서 Ray 발사
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Ray가 뭔가에 맞았으면 (기능 작동할지 모르겠네)
            if (Physics.Raycast(ray, out hit))
            {
                //맞는 위치에 타워 배치
                PlaceTower(hit.point);
            }
        }
    }
    
    //타워 생성 함수
    public bool PlaceTower(Vector3 position)
    {
        Debug.Log("타워 배치 함수 시작");

        // 골드 조건 추가
        if (_gold < _towerCost)
        {
            Debug.Log("골드가 부족합니다.");
            return false;
        }
        
        if (CanPlaceTower(position) == false)
        {
            return false; // 타워가 있다면 타워가 안생김
        }

        //타워 생성 (Instantiate 복제해서 새오브젝트에 넣기)
        GameObject newTowerObj = Instantiate(_towerPrefab, position, Quaternion.identity);
        Tower newTower = newTowerObj.GetComponent<Tower>();
        
        //리스트에 추가 (새로운 타워를 리스트에 배열로 추가)
        _placedTowers.Add(newTower);

        // 골드 차감
        _gold -= _towerCost;

        Debug.Log("타워 배치 완료! 남은 골드 :" + _gold);
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
        if(CheckTowerExists(position) == false)  // 타워가 없으면 false 반환
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

        foreach(Collider col in colliders)
        {
            if(col.CompareTag("Tower")) // 타겟이 타워인 친구들만 체크 에정
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
            _gold += refund;

            // 리스트에서 제거
            _placedTowers.Remove(tower);

            // 오브젝트 삭제
            Destroy(tower.gameObject);

            Debug.Log("타워 삭제! 환불 :" + refund + "/남은 골드 :" + _gold);
        }
    }
}
