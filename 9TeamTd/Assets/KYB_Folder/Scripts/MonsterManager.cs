using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    [Header("Setting")] 
    [SerializeField] private Monster monsterPrefab;

    [SerializeField] private Transform baseTransform;
    
    // 현재 레벨(스테이지) 스폰 목록
    private List<MonsterSpawnDatas> currentLevelSchedule = new List<MonsterSpawnDatas>();

    private float gameTime = 0f;
    private bool isRunning = false;
    
    // 몬스터 스폰 기록용
    private Dictionary<int, float> lastSpawnTimeById = new Dictionary<int, float>();
    
    // 오브젝트 풀
    private Stack<Monster> pool = new Stack<Monster>();

    private void Awake()
    {
        /*
        // 싱글톤 초기화
        if (Instance != null || Instance != this)
        {
            Debug.LogError($"MonsterManager가 {Instance.gameObject.name}에 중복되어 있음");
            Destroy(gameObject);
            return;
        }
        */
        Instance = this;
        Debug.Log("싱글톤 등록 완료 [MonsterManager]");

        if (baseTransform == null)
        {
            GameObject baseObj = GameObject.Find("Base");
            if (baseObj != null) baseTransform = baseObj.transform;
        }
        // DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadLevelData();
    }

    public void LoadLevelData()
    {
        currentLevelSchedule.Clear();
        lastSpawnTimeById.Clear();
        
        // JsonManager에서 스폰 데이터를 가져온다
        for (int id = 2100; id <= 2200; id++) // << 엑셀에 적은 ID 범위에 맞게 수정
        {
            var data = JsonManager.instanceJsonManger.GetMonsterSpawnData(id);

            if (data != null)
            {
                currentLevelSchedule.Add(data);
            }
        }

        gameTime = 0f;
        isRunning = true;
        Debug.Log($"[MonsterManager] 게임 시작, 총 {currentLevelSchedule}개의 스폰 패턴 가동");
    }

    private void Update()
    {
        if (!isRunning) return;

        gameTime += Time.deltaTime;

        // 스케줄 리스트를 검사
        foreach (var pattern in currentLevelSchedule)
        {
            // 1. 지금이 이 몬스터가 나올 시간대인가? (Start <= 현재 < End)
            if (gameTime >= pattern.startTime && gameTime < pattern.endTime)
            {
                // 2. 쿨타임(Interval)이 지났는가?
                if (!lastSpawnTimeById.ContainsKey(pattern.id))
                    lastSpawnTimeById[pattern.id] = -999f; // 처음엔 무조건 스폰
                
                if (gameTime - lastSpawnTimeById[pattern.id] >= pattern.interval)
                {
                    SpawnMonster(pattern.id);
                    lastSpawnTimeById[pattern.id] = gameTime; // 마지막 스폰 시간 갱신
                }
            }
        }
    }
    
    // 몬스터 스폰 로직
    private void SpawnMonster(int monsterId)
    {
        if (!GridSystem.Instance.TryGetRandomSpawnCell(out Cell spawnCell))
        {
            return;
        }
        
        // 그리드 좌표 -> 월드 좌표 변환
        Vector3 spawnPos = GridSystem.Instance.CellToWorld(spawnCell);
        
        // 기본 스탯 + 보상 정보 가져오기
        MonsterDatas stats = JsonManager.instanceJsonManger.GetMonsterData(monsterId, 1);
        MonsterResourcesDatas rewards = JsonManager.instanceJsonManger.GetMonsterResourcesData(monsterId);
        
        if (stats == null || rewards == null)
        {
            Debug.LogError($"[MonsterManager] ID {monsterId}의 데이터가 없습니다.");
        }

        Monster newMonster = GetFromPool();
        newMonster.transform.position = spawnPos;
        
        // 몬스터 초기화
        newMonster.Initialize(stats, rewards, baseTransform);

    }

    // 오브젝트 풀링
    private Monster GetFromPool()
    {
        if (pool.Count > 0)
        {
            Monster m = pool.Pop();
            m.gameObject.SetActive(true);
            return m;
        }
        else
        {
            // 풀이 비었으면 새로 생성
            Monster m = Instantiate(monsterPrefab, transform);
            m.OnDeath += ReturnToPool;
            return m;
        }
    }

    public void ReturnToPool(Monster monster)
    {
        monster.gameObject.SetActive(false);
        pool.Push(monster);
    }
}
