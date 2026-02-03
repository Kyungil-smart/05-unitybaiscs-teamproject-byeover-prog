using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 몬스터 스폰 테스트용 컨트롤러
/// 
/// 사용법:
/// 1. 빈 GameObject에 이 스크립트 추가
/// 2. Inspector에서 테스트할 몬스터 ID 설정
/// 3. 키보드 단축키로 스폰
/// </summary>
public class MonsterSpawnTester : MonoBehaviour
{
    [Header("=== 테스트 설정 ===")]
    [Tooltip("테스트할 몬스터 ID 리스트")]
    public List<int> testMonsterIds = new List<int> { 1000, 1001, 1002 };

    [Tooltip("현재 선택된 몬스터 인덱스")]
    [SerializeField] private int currentMonsterIndex = 0;

    [Header("=== 단축키 설정 ===")]
    [Tooltip("몬스터 스폰 키")]
    public KeyCode spawnKey = KeyCode.Space;

    [Tooltip("다음 몬스터로 전환")]
    public KeyCode nextMonsterKey = KeyCode.RightArrow;

    [Tooltip("이전 몬스터로 전환")]
    public KeyCode prevMonsterKey = KeyCode.LeftArrow;

    [Tooltip("연속 스폰 토글")]
    public KeyCode toggleContinuousSpawnKey = KeyCode.C;

    [Header("=== 연속 스폰 설정 ===")]
    [SerializeField] private bool isContinuousSpawn = false;

    [Tooltip("연속 스폰 간격 (초)")]
    [SerializeField] private float continuousSpawnInterval = 1.0f;

    private float lastSpawnTime = 0f;

    private void Start()
    {
        UpdateCurrentMonsterInfo();
    }

    private void Update()
    {
        HandleInput();
        HandleContinuousSpawn();
    }

    private void HandleInput()
    {
        // 스폰 키
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnCurrentMonster();
        }

        // 몬스터 전환
        if (Input.GetKeyDown(nextMonsterKey))
        {
            NextMonster();
        }

        if (Input.GetKeyDown(prevMonsterKey))
        {
            PreviousMonster();
        }

        // 연속 스폰 토글
        if (Input.GetKeyDown(toggleContinuousSpawnKey))
        {
            ToggleContinuousSpawn();
        }
    }

    /// <summary>
    /// 연속 스폰 모드 토글
    /// </summary>
    public void ToggleContinuousSpawn()
    {
        isContinuousSpawn = !isContinuousSpawn;

        if (isContinuousSpawn)
        {
            lastSpawnTime = Time.time;
        }
    }

    private void HandleContinuousSpawn()
    {
        if (!isContinuousSpawn) return;

        if (Time.time - lastSpawnTime >= continuousSpawnInterval)
        {
            SpawnCurrentMonster();
            lastSpawnTime = Time.time;
        }
    }

    /// <summary>
    /// 현재 선택된 몬스터를 스폰
    /// </summary>
    public void SpawnCurrentMonster()
    {
        int monsterId = testMonsterIds[currentMonsterIndex];
        SpawnMonsterById(monsterId);
    }

    /// <summary>
    /// 특정 ID의 몬스터를 스폰 (외부 호출용)
    /// </summary>
    public void SpawnMonsterById(int monsterId)
    {
        // 스폰 위치 결정
        Cell spawnCell;

        if (!GridSystem.Instance.TryGetRandomSpawnCell(out spawnCell))
        {
            return;
        }


        // 데이터 가져오기
        MonsterDatas stats = JsonManager.instanceJsonManger.GetMonsterData(monsterId, 1);
        MonsterResourcesDatas rewards = JsonManager.instanceJsonManger.GetMonsterResourcesData(monsterId);

        GameObject prefabToSpawn = MonsterManager.Instance.monsterPrefab[stats.Type];

        // 월드 좌표로 변환 및 스폰
        Vector3 spawnPos = GridSystem.Instance.CellToWorld(spawnCell);
        spawnPos.y += 1.0f;

        GameObject newObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        Monster newMonster = newObj.GetComponent<Monster>();
        newMonster?.Initialize(stats, rewards, MonsterManager.Instance.baseTransform);
    }

    /// <summary>
    /// 다음 몬스터로 전환
    /// </summary>
    public void NextMonster()
    {
        currentMonsterIndex = (currentMonsterIndex + 1) % testMonsterIds.Count;
        UpdateCurrentMonsterInfo();
    }

    /// <summary>
    /// 이전 몬스터로 전환
    /// </summary>
    public void PreviousMonster()
    {
        currentMonsterIndex--;
        if (currentMonsterIndex < 0)
            currentMonsterIndex = testMonsterIds.Count - 1;

        UpdateCurrentMonsterInfo();
    }

    /// <summary>
    /// 현재 몬스터 정보 업데이트
    /// </summary>
    private void UpdateCurrentMonsterInfo()
    {
        int currentId = testMonsterIds[currentMonsterIndex];
        MonsterDatas stats = JsonManager.instanceJsonManger?.GetMonsterData(currentId, 1);
    }
}
