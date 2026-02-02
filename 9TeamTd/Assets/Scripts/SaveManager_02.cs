using System.Collections;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SaveManager_02 : MonoBehaviour
{
    [System.Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public int gold = 0;
        public int stage = 1;
        public string last_play_time_utc = "";
    }

    [Header("세이브 설정")]
    [SerializeField] private string file_name = "save.json";
    [SerializeField] private bool verbose_logs = true;

    public SaveData Current { get; private set; }
    private string SavePath => Path.Combine(Application.persistentDataPath, file_name);

    public void Init()
    {
        if (verbose_logs) Debug.Log($"[SaveManager] Init path={SavePath}", this);
    }

    public IEnumerator LoadOrCreateRoutine()
    {
        // 한 프레임 쉬어주면 디버깅/안정성에 도움
        yield return null;

        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            Current = JsonUtility.FromJson<SaveData>(json);

            if (Current == null)
                Current = CreateNew();

            if (verbose_logs) Debug.Log("[SaveManager] Load ok", this);
        }
        else
        {
            Current = CreateNew();
            SaveNow();

            if (verbose_logs) Debug.Log("[SaveManager] No save -> created new", this);
        }
    }

    public void SaveNow()
    {
        if (Current == null) Current = CreateNew();

        Current.last_play_time_utc = System.DateTime.UtcNow.ToString("o");

        string json = JsonUtility.ToJson(Current, true);
        File.WriteAllText(SavePath, json);

        if (verbose_logs) Debug.Log("[SaveManager] Saved", this);
    }

    private SaveData CreateNew()
    {
        return new SaveData
        {
            version = 1,
            gold = 0,
            stage = 1,
            last_play_time_utc = System.DateTime.UtcNow.ToString("o")
        };
    }
}
