using UnityEngine;

// 작성자 : 문형근
// 게임 전체 상태 관리 (승리, 패배, 웨이브 등)
// 사용법:
// 1. 빈 오브젝트 만들고 이 스크립트 추가
// 2. 몬스터가 목표 도달하면 GameManager.Instance.TakeDamage() 호출
// 3. 웨이브 클리어하면 GameManager.Instance.WaveCleared() 호출

public class GameManager : MonoBehaviour
{
    // ========== 싱글톤 ==========
    // 다른 스크립트에서 GameManager.Instance로 접근 가능
    public static GameManager Instance;

    // ========== 게임 상태 ==========
    public enum GameState
    {
        Playing,    // 게임 진행 중
        Victory,    // 승리
        Defeat      // 패배
    }

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

    // ========== 초기화 ==========
    void Awake()
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

    void Start()
    {
        // 게임 시작 시 초기화
        StartGame();
    }

    // ========== 게임 시작 ==========
    public void StartGame()
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
        Debug.Log($"[GameManager] 모든 웨이브 클리어! ({_currentWave}/{_totalWaves})");

        // TODO: 승리 UI 표시
        // TODO: 게임 일시정지 (Time.timeScale = 0)
    }

    // ========== 패배 처리 ==========
    private void OnDefeat()
    {
        _currentState = GameState.Defeat;

        Debug.Log("");
        Debug.Log("╔══════════════════════════════╗");
        Debug.Log("║                              ║");
        Debug.Log("║       💀 패배... 💀          ║");
        Debug.Log("║                              ║");
        Debug.Log("╚══════════════════════════════╝");
        Debug.Log("");
        Debug.Log($"[GameManager] 생명이 0이 되었습니다!");

        // TODO: 패배 UI 표시
        // TODO: 게임 일시정지 (Time.timeScale = 0)
    }

    // ========== 게임 재시작 ==========
    public void RestartGame()
    {
        Debug.Log("[GameManager] 게임 재시작!");
        StartGame();

        // TODO: 씬 다시 로드하거나 오브젝트 초기화
    }

    // ========== 디버그용 테스트 (키보드 입력) ==========
    void Update()
    {
        // F1: 데미지 테스트
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("[GameManager] F1 - 데미지 테스트");
            TakeDamage(1);
        }

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
    }
}
