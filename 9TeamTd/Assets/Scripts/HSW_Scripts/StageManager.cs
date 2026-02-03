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
    public int ClearOutGameGem = 0;
    public int DefeatOutGameGem = 0;

    // 베이스 스폰 위치 관련
    [SerializeField] private int spawnBaseID = 0;
    [SerializeField] private int spawnBaseLevel = 1;
    [SerializeField] private string baseAddress = "Tower/Base_00_First";
    [SerializeField] private Transform basePosition;
    // [SerializeField] private Transform bRotation;   // 베이스 로테이션

    [Header("For GamePlay")]
    [SerializeField] private float currentTime = 0;
    public int currentMinutes = 0;
    public float currentSeconds = 0;
    public float stageEndTime = 0;  // 추후 private 로 수정 필요




    [Header("아이템 드랍 설정")] 
    [SerializeField] private List<GameObject> itemPrefabs;




    // ========== 싱글톤 ==========
    // 다른 스크립트에서 GameManager.Instance로 접근 가능
    public static StageManager Instance { get; private set; }
   

    // 현재 게임 상태
    private GameState _currentState = GameState.Playing;
    public GameState CurrentState => _currentState;

    /*
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

    */
    public Action StageClear;
    public Action StageDefeat;

    public static OP<int> gold = new();


    private void Awake()
    {
        Instance = this;

        /*
        // 싱글톤 설정
        if (Instance == null)
        {
            
            Debug.Log("[GameManager] 싱글톤 생성 완료");
        }
        else
        {
            Debug.LogWarning("[GameManager] 이미 인스턴스가 존재합니다. 중복 삭제!");
            Destroy(gameObject);
            return;
        }
        */

    }


    private void Start()
    {

        // 게임 시작 시 초기화
        stageEndTime = stageEndTimeForReset;
        SpawnBase();
        StartInGame();
        gold.Value = startGold;
        GameManager.Instance.ChangeBGM();
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
        currentTime = Time.timeSinceLevelLoad;
        /*
        // 플레이 시간 계산
        
        currentTime += Time.fixedDeltaTime;
        currentSeconds += Time.fixedDeltaTime;


        if (currentSeconds >= 60)
        {
            currentMinutes += 1;
            currentSeconds = 0;
        }
        */


        if (stageEndTimeForReset <= Time.timeSinceLevelLoad)
        {
            OnVictory();

        }

    }


    // 타워 생성 함수
    private void SpawnBase()
    {
        // 게임 오브젝트 정보 및 주소 가져오기 (베이스의 경우)
        spawnBaseID = GameManager.Instance.SelectedBaseID.Value;
        baseAddress = JsonManager.instanceJsonManger.ReturnTowerAddress(spawnBaseID);


        // 위치를 받고 오브젝트 생성
        Vector3 curBasePos = basePosition.position;
        GameObject spawnBase = Instantiate(Resources.Load<GameObject>(baseAddress), curBasePos, Quaternion.identity);
        spawnBase.GetComponent<TowerStats>().id = spawnBaseID;
        spawnBase.GetComponent<TowerStats>().level = spawnBaseLevel;
        spawnBase.GetComponent<TowerStats>().Init();

        /*
        Tower newTower = spawnBase.GetComponent<Tower>().GetStats();

        TowerStats towerStats = spawnBase.GetComponent<TowerStats>();

        TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(spawnBaseID, spawnBaseLevel);
        // spawnBaseID 기반 오브젝트 선택하기
        if (data != null)
        {
            // 가져온 데이터 주입
            towerStats.SetupValue(data);
        }
        */
    }




    // ========== 게임 시작 ==========
    public void StartInGame()
    {
        /*
        _currentLife = _maxLife;
        _currentWave = 0;
        _currentState = GameState.Playing;
        */
        Debug.Log("============================================");
        Debug.Log("[GameManager] 게임 시작!");
        //Debug.Log($"  - 생명: {_currentLife}/{_maxLife}");
        //Debug.Log($"  - 총 웨이브: {_totalWaves}");
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
    /*
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
    */ 

    // ========== 승리 처리 ==========
    private void OnVictory()
    {
        _currentState = GameState.Victory;

        // Debug.Log($"[GameManager] 모든 웨이브 클리어! ({_currentWave}/{_totalWaves})");

        // TODO: 승리 UI 표시
        // TODO: 게임 일시정지 (Time.timeScale = 0)

        StageClear?.Invoke();
        PauseFunction();
        SoundManager.Instance.PlayWinBgm();
        SaveManager.instance.SaveData(1, ClearOutGameGem);
        GameManager.Instance.Init();

    }



    // ========== 패배 처리 ==========
    public void OnDefeat()
    {
        _currentState = GameState.Defeat;

        // Debug.Log($"[GameManager] 생명이 0이 되었습니다!");

        StageDefeat?.Invoke();
        PauseFunction();

        SoundManager.Instance.PlayLoseBgm();
        SaveManager.instance.SaveData(DefeatOutGameGem);
        //GameManager.Instance.Init();


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





    public void GetGold(int value)
    {
        if (gold == null) gold = new OP<int>();

        gold.Value += value;
        Debug.Log($"[StageManager] 골드 획득! +{value}");

    }


    public void TryDropItem(string itemId, float prob, Vector3 spawnPos)
    {
        if (string.IsNullOrEmpty(itemId) || prob <= 0) return;

        // 확률 체크
        if (UnityEngine.Random.value <= prob)
        {
            SpawnItem(itemId, spawnPos);
        }
    }

    private void SpawnItem(string itemId, Vector3 dropPos)
    {
        // string ID -> int 변환
        if (int.TryParse(itemId, out int index))
        {
            // 리스트 범위 체크
            if (index >= 0 && index < itemPrefabs.Count)
            {
                GameObject prefab = itemPrefabs[index];
                if (prefab != null)
                {
                    // 아이템 생성 (위치 살짝 위로 보정)
                    Vector3 finalPos = dropPos + new Vector3(0, 1.5f, 0);
                    Instantiate(prefab, finalPos, Quaternion.identity);
                    
                    Debug.Log($"아이템 드랍 성공 (ID: {index})");
                }
            }
            else
            {
                Debug.LogWarning($"아이템 프리팹이 리스트에 없습니다. ID: {index}");
            }
        }
        else
        {
            Debug.LogError($"아이템 ID 변환 실패: {itemId}");
        }
    }
}
