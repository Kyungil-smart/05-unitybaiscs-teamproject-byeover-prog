using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

// ========== 게임 상태 ==========
public enum GameState
{
    Playing,    // 게임 진행 중
    Victory,    // 승리
    Defeat      // 패배
}


public class StageManager : MonoBehaviour
{
    [Header("Stage Info")]
    public int stageID = 0;
    public float stageEndTimeForReset = 0;
    public int startGold = 0;
    [SerializeField] private Transform basePosition;

    [Header("For GamePlay")]
    [SerializeField] private float currentTime = 0;
    public int currentMinutes = 0;
    public float currentSeconds = 0;
    public float stageEndTime = 0;  // 추후 private 로 수정 필요



    // ========== 싱글톤 ==========
    // 다른 스크립트에서 GameManager.Instance로 접근 가능
    public static StageManager Instance { get; private set; }


    // 현재 게임 상태
    private GameState _currentState = GameState.Playing;
    public GameState CurrentState => _currentState;

    // ========== 플레이어 생명 ==========
    [Header("플레이어 설정")]
    [SerializeField] private int _maxLife = 10;      // 최대 생명
    private int _currentLife;                         // 현재 생명
    public int CurrentLife => _currentLife;
    public int MaxLife => _maxLife;

    // ========== 웨이브 설정 ==========
    [Header("웨이브 설정")]
    [SerializeField] private int _totalWaves = 10;    // 총 웨이브 수
    private int _currentWave = 0;                      // 현재 웨이브
    public int CurrentWave => _currentWave;
    public int TotalWaves => _totalWaves;


    public Action StageClear;
    public Action StageDefeat;

    public static OP<int> gold = new();


    private void Awake()
    {

        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[GameManager] 싱글톤 생성 완료");
        }
        else
        {
            Debug.LogWarning("[GameManager] 이미 인스턴스가 존재합니다. 중복 삭제!");
            Destroy(gameObject);
            return;
        }


    }


    private void Start()
    {

        // 게임 시작 시 초기화
        stageEndTime = stageEndTimeForReset;
        StartInGame();
        gold.Value = startGold;
    }



    void Update()
    {

        // ========== 디버그용 테스트 (키보드 입력) ==========
        /*
        // F1: 데미지 테스트
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("[GameManager] F1 - 데미지 테스트");
            TakeDamage(1);
        }
        */

        /*
        // F2: 웨이브 클리어 테스트
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("[GameManager] F2 - 웨이브 클리어 테스트");
            WaveCleared();
        }

        // F3: 게임 재시작 테스트
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("[GameManager] F3 - 게임 재시작 테스트");
            RestartGame();
        }
        */

    }


    // 인게임 로직
    private void FixedUpdate()
    {

        // 플레이 시가 계산
        currentTime += Time.fixedDeltaTime;
        currentSeconds += Time.fixedDeltaTime;


        if (currentSeconds >= 60)
        {
            currentMinutes += 1;
            currentSeconds = 0;
        }



        if (stageEndTime <= currentTime)
        {
            OnVictory();

        }

    }



    // ========== 게임 시작 ==========
    public void StartInGame()
    {
        _currentLife = _maxLife;
        _currentWave = 0;
        _currentState = GameState.Playing;

        Debug.Log("============================================");
        Debug.Log("[GameManager] 게임 시작!");
        Debug.Log($"  - 생명: {_currentLife}/{_maxLife}");
        Debug.Log($"  - 총 웨이브: {_totalWaves}");
        Debug.Log("============================================");
    }

    /*
    // ========== 데미지 받기 (몬스터가 목표 도달 시 호출) ==========
    public void TakeDamage(int damage = 1)
    {
        // 이미 게임 끝났으면 무시
        if (_currentState != GameState.Playing)
        {
            Debug.Log("[GameManager] 게임이 이미 끝났습니다.");
            return;
        }

        // 생명 감소
        _currentLife -= damage;
        Debug.Log($"[GameManager] 데미지 받음! -{damage} (남은 생명: {_currentLife}/{_maxLife})");

        // 생명이 0 이하면 패배
        if (_currentLife <= 0)
        {
            _currentLife = 0;
            OnDefeat();
        }
    }
    */


    // ========== 웨이브 클리어 (웨이브 끝나면 호출) ==========
    public void WaveCleared()
    {
        // 이미 게임 끝났으면 무시
        if (_currentState != GameState.Playing)
        {
            Debug.Log("[GameManager] 게임이 이미 끝났습니다.");
            return;
        }

        _currentWave++;
        Debug.Log($"[GameManager] 웨이브 클리어! ({_currentWave}/{_totalWaves})");

        // 모든 웨이브 클리어하면 승리
        if (_currentWave >= _totalWaves)
        {
            OnVictory();
        }
    }

    // ========== 승리 처리 ==========
    private void OnVictory()
    {
        _currentState = GameState.Victory;

        Debug.Log("");
        Debug.Log("╔══════════════════════════════╗");
        Debug.Log("║                              ║");
        Debug.Log("║       🎉 승리! 🎉            ║");
        Debug.Log("║                              ║");
        Debug.Log("╚══════════════════════════════╝");
        Debug.Log("");
        // Debug.Log($"[GameManager] 모든 웨이브 클리어! ({_currentWave}/{_totalWaves})");

        // TODO: 승리 UI 표시
        // TODO: 게임 일시정지 (Time.timeScale = 0)

        StageClear?.Invoke();
        PauseFunction();
    }



    // ========== 패배 처리 ==========
    public void OnDefeat()
    {
        _currentState = GameState.Defeat;

        Debug.Log("");
        Debug.Log("╔══════════════════════════════╗");
        Debug.Log("║                              ║");
        Debug.Log("║       💀 패배... 💀          ║");
        Debug.Log("║                              ║");
        Debug.Log("╚══════════════════════════════╝");
        Debug.Log("");
        // Debug.Log($"[GameManager] 생명이 0이 되었습니다!");

        StageDefeat?.Invoke();
        PauseFunction();

        // TODO: 패배 UI 표시
        // TODO: 게임 일시정지 (Time.timeScale = 0)
    }

    // ========== 게임 재시작 ==========
    public void RestartGame()
    {
        Debug.Log("[GameManager] 게임 재시작!");
        StartInGame();

        // TODO: 씬 다시 로드하거나 오브젝트 초기화
    }



    public void PauseFunction()
    {
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }
    }


    [Header("아이템 드랍 설정")]
    [SerializeField] private List<GameObject> dropItemPrefabs;


    public void GetGold(int value)
    {
        if (gold == null) gold = new OP<int>();

        gold.Value = value;
        Debug.Log($"[StageManager] 골드 획득! +{value}");

        // UI 업데이트 이벤트를 호출하거나 연결하면 .. 될듯싶은데
    }

    public void TryDropItem(string itemId, float prob, Vector3 spawnPos)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        // 확률 체크
        if (UnityEngine.Random.value < prob)
        {
            SpawnItem(itemId, spawnPos);
        }
    }

    private void SpawnItem(string itemId, Vector3 pos)
    {

    }
}
