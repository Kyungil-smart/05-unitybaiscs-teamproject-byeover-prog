using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;

    [Header("Setting")]
    public GameObject monsterPrefab;
    public Transform baseTransform;
    
    // 스폰 스케줄 리스트
    private List<MonsterSpawnDatas> _currentLevelSchedule = new List<MonsterSpawnDatas>();
    // 마지막 스폰 시간 기억용
    private Dictionary<int, float> _lastSpawnTimeById = new Dictionary<int, float>();

    private float _gameTime = 0f;
    private bool _isRunning = false;
    
    private IObjectPool<Monster> monsterPool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Base 자동 할당 (없을 경우)
        if (baseTransform == null)
        {
            GameObject baseObj = GameObject.FindWithTag("Base");
            if (baseObj != null) baseTransform = baseObj.transform;
        }

        monsterPool = new ObjectPool<Monster>(
            CreateMonster,    // 없을 때 새로 만드는 함수
            OnGetMonster,     // 꺼낼 때 실행할 함수
            OnReleaseMonster, // 반납할 때 실행할 함수
            OnDestroyMonster, // 풀이 꽉 찼거나 에러날 때 삭제 함수
            maxSize: 100      // 최대 보관 개수(늘려도 됨)
        );

    }

    private Monster CreateMonster()
    {
        // 프리팹 생성 후 Monster 컴포넌트 리턴
        GameObject obj = Instantiate(monsterPrefab);
        Monster monster = obj.GetComponent<Monster>();

        return monster;
    }
    
    private void OnGetMonster(Monster monster)
    {
        monster.gameObject.SetActive(true); // 켜기
    }

    private void OnReleaseMonster(Monster monster)
    {
        monster.gameObject.SetActive(false); // 끄기 (대기 상태)
    }

    private void OnDestroyMonster(Monster monster)
    {
        Destroy(monster.gameObject);
    }

    private void Start()
    {
        LoadLevelData();
    }
    
    public void LoadLevelData()
    {
        _currentLevelSchedule.Clear();
        _lastSpawnTimeById.Clear();
        
        // 엑셀 데이터 ID 범위에 맞춰 스폰 스케줄 로드 (2100 ~ 2200)
        for (int id = 2100; id <= 2200; id++) 
        {
            var data = JsonManager.instanceJsonManger.GetMonsterSpawnData(id);
            if (data != null)
            {
                _currentLevelSchedule.Add(data);
            }
        }

        _gameTime = 0f;
        _isRunning = true;
        
        Debug.Log($"스폰 데이터 로드 완료: 총 {_currentLevelSchedule.Count}개의 패턴");
    }

    private void Update()
    {
        if (!_isRunning) return;

        _gameTime += Time.deltaTime;
        
        foreach (var pattern in _currentLevelSchedule)
        {
            // 스폰 시간 범위 체크
            if (_gameTime >= pattern.startTime && _gameTime < pattern.endTime)
            {
                // 딕셔너리 초기화
                if (!_lastSpawnTimeById.ContainsKey(pattern.id))
                    _lastSpawnTimeById[pattern.id] = -999f; 

                // 쿨타임 체크 (현재시간 - 마지막스폰시간 >= 간격)
                if (_gameTime - _lastSpawnTimeById[pattern.id] >= pattern.interval)
                {
                    SpawnMonster(pattern.id);
                    _lastSpawnTimeById[pattern.id] = _gameTime; 
                }
            }
        }
    }

    private void SpawnMonster(int monsterId)
    {
        if (GridSystem.Instance == null) return;

        // 스폰 위치(빈 셀) 찾기
        if (!GridSystem.Instance.TryGetRandomSpawnCell(out Cell spawnCell))
        {
            Debug.LogWarning("빈 스폰 위치를 찾을 수 없습니다.");
            return; 
        }

        // 데이터 가져오기 (기본 레벨 1)
        MonsterDatas stats = JsonManager.instanceJsonManger.GetMonsterData(monsterId, 1);
        MonsterResourcesDatas rewards = JsonManager.instanceJsonManger.GetMonsterResourcesData(monsterId);

        if (stats == null) return;

        // 풀에서 꺼내기
        Monster newMonster = monsterPool.Get();
        
        // 위치 및 초기화
        Vector3 spawnPos = GridSystem.Instance.CellToWorld(spawnCell);
        // 땅에 파묻히면 Y 값 조정
        spawnPos.y += 1.0f;
        
        newMonster.transform.position = spawnPos;
        newMonster.Initialize(stats, rewards, baseTransform);
    }
    
    // 몬스터가 죽거나 도착했을 때 부르는 함수
    public void ReturnMonster(Monster monster)
    {
        monsterPool.Release(monster);
        
        monster.gameObject.SetActive(false);
    }
}