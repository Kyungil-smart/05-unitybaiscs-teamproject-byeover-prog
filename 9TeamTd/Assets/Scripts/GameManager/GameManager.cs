using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 작성자 : 문형근
// 수정자 : 한성우
// 게임 전체 상태 관리 (승리, 패배, 웨이브 등)
// 사용법:
// 1. 빈 오브젝트 만들고 이 스크립트 추가
// 2. 몬스터가 목표 도달하면 GameManager.Instance.TakeDamage() 호출
// 3. 웨이브 클리어하면 GameManager.Instance.WaveCleared() 호출

public class GameManager : MonoBehaviour
{
    [Header("Stage Info")]
    public int outGameGold = 0;
    public int SelectedBaseID = 0;
    public int SelectedstageID = 0;



    // ========== 싱글톤 ==========
    // 다른 스크립트에서 GameManager.Instance로 접근 가능
    public static GameManager Instance; /*{ get; private set; }*/

    // 아웃 게임 로직

    [Header("부팅 설정")]
    [SerializeField] private bool verbose_logs = true;

    [Tooltip("부팅이 끝나기 전에는 게임 진입 버튼을 막는 용도")]
    [SerializeField] private bool boot_completed = false;

    [Header("씬 이름")]
    [SerializeField] private string title_scene_name = "Title";
    [SerializeField] private string ingame_scene_name = "InGame";

    public bool BootCompleted => boot_completed;

    public SaveManager_02 Save { get; private set; }
    public JsonManager_02 Json { get; private set; }




    private void Awake()
    {
        // --- 싱글톤/중복 방지 ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (verbose_logs) Debug.Log("[GameManager] Awake -> DontDestroyOnLoad", this);



        // 매니저 생성 보장
        EnsureManagersExist();



    }


    private void Start()
    {
        // 부팅은 Start에서 (씬 로드 타이밍 안정)
        StartCoroutine(BootRoutine());

        // 게임 시작 시 초기화
        // StartInGame();
    }



    private void EnsureManagersExist()
    {
        // SaveManager
        Save = GetComponentInChildren<SaveManager_02>(true);
        if (Save == null)
        {
            var go = new GameObject("@SaveManager");
            go.transform.SetParent(transform);
            Save = go.AddComponent<SaveManager_02>();
        }

        // JsonManager
        Json = GetComponentInChildren<JsonManager_02>(true);
        if (Json == null)
        {
            var go = new GameObject("@JsonManager");
            go.transform.SetParent(transform);
            Json = go.AddComponent<JsonManager_02>();
        }
    }

    private IEnumerator BootRoutine()
    {

        boot_completed = false;

        if (verbose_logs) Debug.Log("[GameManager] Boot start", this);

        // 1) Save 로드가 최우선
        Save.Init();
        yield return Save.LoadOrCreateRoutine();

        // 2) Json 정의 데이터 초기화(테이블 로드 자리)
        Json.Init();
        yield return Json.LoadTablesRoutine();

        boot_completed = true;
        if (verbose_logs) Debug.Log("[GameManager] Boot complete", this);

    }


    // -----------------------------
    // Title UI 버튼에 연결할 함수
    // -----------------------------
    public void StartGame()
    {
        if (!boot_completed)
        {
            Debug.LogWarning("[GameManager] Boot가 끝나지 않았습니다. 잠시 후 다시 시도하세요.", this);
            return;
        }

        if (verbose_logs) Debug.Log($"[GameManager] LoadScene -> {ingame_scene_name}", this);
        SceneManager.LoadScene(ingame_scene_name);
    }

    public void ReturnToTitle()
    {
        if (verbose_logs) Debug.Log($"[GameManager] LoadScene -> {title_scene_name}", this);
        SceneManager.LoadScene(title_scene_name);
    }


   
}
