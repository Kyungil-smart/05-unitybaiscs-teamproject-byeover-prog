using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임용 몬스터 스폰 시스템
/// 10분 동안 1초마다 몬스터를 랜덤 스폰
/// 시간에 따라 강한 몬스터가 출현하는 확률 증가
/// </summary>
public class GameMonsterSpawner : MonoBehaviour
{
    int weakMonsterStartId = 2100;
    int weakMonsterEndId = 2111;

    int midMonsterStartId = 2120;
    int midMonsterEndId = 2129;

    int strongMonsterStartId = 2130;
    int strongMonsterEndId = 2135;

    [Header("=== 스폰 설정 ===")]
    [Tooltip("스폰 지속 시간 (초)")]
    [SerializeField] private float spawnDuration = 600f; // 10분

    [Tooltip("스폰 간격 (초)")]
    [SerializeField] private float spawnInterval = 1f;

    [Header("=== 난이도 곡선 ===")]
    [Tooltip("강한 몬스터가 나오기 시작하는 시간 (초)")]
    [SerializeField] private float strongMonsterStartTime = 300f; // 5분부터

    [Tooltip("강한 몬스터 최대 확률 (0~1)")]
    [SerializeField] private float maxStrongMonsterChance = 0.3f;

    [Header("=== 디버그 정보 ===")]
    [SerializeField] private bool isSpawning = false;
    [SerializeField] private float elapsedTime = 0f;

    // 코루틴 캐싱
    private WaitForSeconds waitOneSecond;
    private Coroutine spawnCoroutine;

    private void Awake()
    {
        // 1초 대기 코루틴 캐싱
        waitOneSecond = new WaitForSeconds(spawnInterval);
    }

    /// <summary>
    /// 스폰 시작
    /// </summary>
    public void StartSpawning()
    {
        ResetStats();
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 스폰 중지
    /// </summary>
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        isSpawning = false;
    }

    /// <summary>
    /// 통계 초기화
    /// </summary>
    private void ResetStats()
    {
        elapsedTime = 0f;
    }

    /// <summary>
    /// 메인 스폰 루틴
    /// </summary>
    private IEnumerator SpawnRoutine()
    {
        isSpawning = true;

        while (elapsedTime < spawnDuration)
        {
            // 몬스터 스폰
            SpawnRandomMonster();

            // 1초 대기 (캐싱된 객체 사용)
            yield return waitOneSecond;

            elapsedTime += spawnInterval;
        }

        // 스폰 완료
        isSpawning = false;
    }

    /// <summary>
    /// 현재 시간에 맞는 랜덤 몬스터 스폰
    /// </summary>
    private void SpawnRandomMonster()
    {
        int monsterId = SelectMonsterIdByTime();
        
        // -1이면 스폰하지 않음 (67% 확률)
        if (monsterId == -1)
        {
            return;
        }
        
        SpawnMonsterById(monsterId);
    }

    /// <summary>
    /// 시간에 따른 몬스터 ID 선택
    /// </summary>
    /// <returns>스폰할 몬스터 ID, 스폰하지 않을 경우 -1</returns>
    private int SelectMonsterIdByTime()
    {
        float progressRatio = elapsedTime / spawnDuration;
        float randomValue = Random.value;

        // 1단계 (0~10%)
        if (progressRatio < 0.1f)
        {
            if (Random.value > 0.2f) return -1; // 스폰 확률: 20%
            return Random.Range(weakMonsterStartId, weakMonsterEndId + 1); // 약한 몬스터: 100%
        }
        // 2단계 (10~25%)
        else if (progressRatio < 0.25f)
        {
            if (Random.value > 0.4f) return -1; // 스폰 확률: 40%
            
            if (randomValue < 0.1f) // 중간 몬스터: 10%
                return Random.Range(midMonsterStartId, midMonsterEndId + 1);
            else // 약한 몬스터: 90%
                return Random.Range(weakMonsterStartId, weakMonsterEndId + 1);
        }
        // 3단계 (25~55%)
        else if (progressRatio < 0.55f)
        {
            if (Random.value > 0.6f) return -1; // 스폰 확률: 60%
            
            if (randomValue < 0.25f) // 중간 몬스터: 25%
                return Random.Range(midMonsterStartId, midMonsterEndId + 1);
            else // 약한 몬스터: 75%
                return Random.Range(weakMonsterStartId, weakMonsterEndId + 1);
        }
        // 4단계 (55~100%)
        else
        {
            if (Random.value > 0.75f) return -1; // 스폰 확률: 75%
            
            if (elapsedTime >= strongMonsterStartTime && randomValue < 0.02f) // 강한 몬스터: 2%
                return Random.Range(strongMonsterStartId, strongMonsterEndId + 1);
            else if (randomValue < 0.40f) // 중간 몬스터: 38%
                return Random.Range(midMonsterStartId, midMonsterEndId + 1);
            else // 약한 몬스터: 60%
                return Random.Range(weakMonsterStartId, weakMonsterEndId + 1);
        }
    }

    /// <summary>
    /// 특정 ID의 몬스터를 스폰
    /// </summary>
    private void SpawnMonsterById(int monsterId)
    {
        // 스폰 위치 찾기
        if (!GridSystem.Instance.TryGetRandomSpawnCell(out Cell spawnCell))
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
#if UNITY_EDITOR
        Debug.Log("랜덤 몬스터 소환 완료");
#endif
    }

    private void Start()
    {
        StartSpawning();
    }
}
