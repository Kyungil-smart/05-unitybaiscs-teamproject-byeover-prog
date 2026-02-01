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
        // 싱글톤 초기화
        if (Instance != null || Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        
    }

    public void LoadLevelData()
    {
        currentLevelSchedule.Clear();
        lastSpawnTimeById.Clear();
        
        // JsonManager에서 스폰 데이터를 가져온다

        for (int id = 1100; id <= 1110; id++)
        {
            var data = JsonManager.instanceJsonManger.GetMonsterSpawnData(id);

            if (data != null)
            {
                currentLevelSchedule.Add(data);
            }
        }
    }
}
