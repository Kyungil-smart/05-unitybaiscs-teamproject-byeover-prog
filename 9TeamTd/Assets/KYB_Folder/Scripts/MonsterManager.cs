using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;

    [Header("Setting")]
    public List<GameObject> monsterPrefab;
    public Transform baseTransform;
    
    // 스폰 스케줄 리스트
    private List<MonsterSpawnDatas> _currentLevelSchedule = new List<MonsterSpawnDatas>();
    // 마지막 스폰 시간 기억용
    private Dictionary<int, float> _lastSpawnTimeById = new Dictionary<int, float>();

    private float _gameTime = 0f;
    private bool _isRunning = false;

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

        if (stats == null) Debug.LogError($"[MonsterManager] ID: {monsterId}의 스탯 데이터가 Null입니다.");

        if (rewards == null)
        {
            Debug.LogError($"[MonsterManager] ID: {monsterId}의 리소스(Reward) 데이터가 Null입니다.");
        }
        else
        {
            Debug.Log($"[MonsterManager] 데이터 로드 성공, ID: {monsterId}, 골드: {rewards.gold}");
        }
        

        if (stats.Type >= monsterPrefab.Count)
        {
            Debug.LogError($"몬스터 타입 {stats.Type}번에 대항하는 프리팹이 리스트에 없습니다.");
            return;
        }

        GameObject prefabToSpawn = monsterPrefab[stats.Type];
        
        // 위치 및 초기화
        Vector3 spawnPos = GridSystem.Instance.CellToWorld(spawnCell);
        spawnPos.y += 1.0f;
        
        GameObject newObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        
        Monster newMonster = newObj.GetComponent<Monster>();
        if (newMonster != null)
        {
            newMonster.Initialize(stats, rewards, baseTransform);
        }
    }

}