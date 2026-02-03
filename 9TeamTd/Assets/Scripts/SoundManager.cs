using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public sealed class SoundManager : MonoBehaviour
{
    // =========================
    // 싱글톤(씬 전환 유지)
    // =========================
    public static SoundManager Instance { get; private set; }

    // =========================
    // 오디오 소스 슬롯
    // - BGM 채널을 분리해서 충돌 방지
    // =========================
    [Header("AudioSource 슬롯")]
    [Tooltip("메인 BGM 전용 AudioSource")]
    [SerializeField] private AudioSource main_bgm_source = null;

    [Tooltip("전투 BGM 전용 AudioSource")]
    [SerializeField] private AudioSource battle_bgm_source = null;

    [Tooltip("승리 BGM 전용 AudioSource(한번만 재생)")]
    [SerializeField] private AudioSource win_bgm_source = null;

    [Tooltip("패배 BGM 전용 AudioSource(한번만 재생)")]
    [SerializeField] private AudioSource lose_bgm_source = null;

    // =========================
    // 클립 리스트 슬롯
    // =========================
    [Header("BGM 리스트(여러 개 넣기)")]
    [Tooltip("메인 씬 BGM 리스트 (랜덤 루프)")]
    [SerializeField] private List<AudioClip> main_bgm_clips = new List<AudioClip>();

    [Tooltip("전투 씬 BGM 리스트 (랜덤 루프)")]
    [SerializeField] private List<AudioClip> battle_bgm_clips = new List<AudioClip>();

    [Tooltip("승리 BGM 리스트 (랜덤 1개, 1회 재생)")]
    [SerializeField] private List<AudioClip> win_bgm_clips = new List<AudioClip>();

    [Tooltip("패배 BGM 리스트 (랜덤 1개, 1회 재생)")]
    [SerializeField] private List<AudioClip> lose_bgm_clips = new List<AudioClip>();

    [Header("디버그")]
    [SerializeField] private bool verbose_logs = false;

    public float soundBGMVolume = 0.3f; // BGM 볼륨

    // 현재 실행 중인 랜덤루프 코루틴 핸들
    private Coroutine _main_loop_co;
    private Coroutine _battle_loop_co;

    private void Awake()
    {
        // 싱글톤 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource가 비어있으면 자동으로 붙여줌(프리팹 세팅 실수 방어)
        EnsureAudioSource(ref main_bgm_source, "MainBGM_Source");
        EnsureAudioSource(ref battle_bgm_source, "BattleBGM_Source");
        EnsureAudioSource(ref win_bgm_source, "WinBGM_Source");
        EnsureAudioSource(ref lose_bgm_source, "LoseBGM_Source");

        // 기본값: 루프는 우리가 코루틴으로 돌릴 거라 AudioSource.loop는 꺼둠
        main_bgm_source.loop = false;
        battle_bgm_source.loop = false;
        win_bgm_source.loop = false;
        lose_bgm_source.loop = false;
    }

    // =========================
    // [Public] 메인/전투/승리/패배 재생 API
    // =========================

    /// <summary>
    /// [메인 BGM]
    /// - 여러 개 중 1개 랜덤 재생
    /// - 끝나면 자동으로 또 랜덤 재생 (무한)
    /// </summary>
    public void PlayMainBgm()
    {
        StopBattleBgm(); // 메인으로 전환 시 전투 루프 중단
        StopOneShotResultBgm(); // 승/패 재생 중이면 중단(원하면 이 줄 빼도 됨)

        main_bgm_source.volume = soundBGMVolume;

        if (_main_loop_co != null) StopCoroutine(_main_loop_co);
        _main_loop_co = StartCoroutine(RandomLoop(main_bgm_source, main_bgm_clips, "Main"));
    }

    /// <summary>
    /// [전투 BGM]
    /// - 여러 개 중 1개 랜덤 재생
    /// - 끝나면 자동으로 또 랜덤 재생 (무한)
    /// </summary>
    public void PlayBattleBgm()
    {
        StopMainBgm();
        StopOneShotResultBgm();

        battle_bgm_source.volume = soundBGMVolume;

        if (_battle_loop_co != null) StopCoroutine(_battle_loop_co);
        _battle_loop_co = StartCoroutine(RandomLoop(battle_bgm_source, battle_bgm_clips, "Battle"));
    }

    /// <summary>
    /// [승리 BGM]
    /// - 여러 개 중 1개 랜덤
    /// - 한번만 재생
    /// </summary>
    public void PlayWinBgm()
    {
        StopMainBgm();
        StopBattleBgm();

        win_bgm_source.volume = soundBGMVolume;

        PlayOneShotRandom(win_bgm_source, win_bgm_clips, "Win");
    }

    /// <summary>
    /// [패배 BGM]
    /// - 여러 개 중 1개 랜덤
    /// - 한번만 재생
    /// </summary>
    public void PlayLoseBgm()
    {
        StopMainBgm();
        StopBattleBgm();

        lose_bgm_source.volume = soundBGMVolume;

        PlayOneShotRandom(lose_bgm_source, lose_bgm_clips, "Lose");
    }

    // =========================
    // [Stop] 필요 시 외부에서 멈출 수도 있게 제공
    // =========================
    public void StopMainBgm()
    {
        if (_main_loop_co != null)
        {
            StopCoroutine(_main_loop_co);
            _main_loop_co = null;
        }
        if (main_bgm_source != null) main_bgm_source.Stop();
    }

    public void StopBattleBgm()
    {
        if (_battle_loop_co != null)
        {
            StopCoroutine(_battle_loop_co);
            _battle_loop_co = null;
        }
        if (battle_bgm_source != null) battle_bgm_source.Stop();
    }

    private void StopOneShotResultBgm()
    {
        if (win_bgm_source != null) win_bgm_source.Stop();
        if (lose_bgm_source != null) lose_bgm_source.Stop();
    }

    // =========================
    // 내부 구현
    // =========================

    // AudioSource가 비어있으면 자식 오브젝트로 생성해서 연결
    private void EnsureAudioSource(ref AudioSource source, string childName)
    {
        if (source != null) return;

        Transform child = transform.Find(childName);
        if (child == null)
        {
            GameObject go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            child = go.transform;
        }

        source = child.GetComponent<AudioSource>();
        if (source == null) source = child.gameObject.AddComponent<AudioSource>();
    }

    // 랜덤 루프 코루틴: 클립 하나 재생 -> 끝나면 또 랜덤
    private System.Collections.IEnumerator RandomLoop(AudioSource src, List<AudioClip> clips, string tag)
    {
        if (src == null) yield break;

        if (clips == null || clips.Count == 0)
        {
            if (verbose_logs) Debug.LogWarning($"[SoundManager] {tag} clips empty.");
            yield break;
        }

        while (true)
        {
            AudioClip next = GetRandomClip(clips);
            if (next == null)
            {
                if (verbose_logs) Debug.LogWarning($"[SoundManager] {tag} random clip is null.");
                yield break;
            }

            src.clip = next;
            src.Play();

            if (verbose_logs) Debug.Log($"[SoundManager] Play {tag}: {next.name}");

            // 재생 길이만큼 대기 (중간 Stop 호출되면 코루틴이 끊김)
            float wait = next.length;
            if (wait <= 0f) wait = 0.1f;
            yield return new WaitForSeconds(wait);
        }
    }

    // 승/패: 랜덤 1개만 재생(한 번)
    private void PlayOneShotRandom(AudioSource src, List<AudioClip> clips, string tag)
    {
        if (src == null) return;

        if (clips == null || clips.Count == 0)
        {
            if (verbose_logs) Debug.LogWarning($"[SoundManager] {tag} clips empty.");
            return;
        }

        AudioClip clip = GetRandomClip(clips);
        if (clip == null)
        {
            if (verbose_logs) Debug.LogWarning($"[SoundManager] {tag} random clip is null.");
            return;
        }

        src.clip = clip;
        src.Play();

        if (verbose_logs) Debug.Log($"[SoundManager] Play {tag} (one-shot): {clip.name}");
    }

    private static AudioClip GetRandomClip(List<AudioClip> clips)
    {
        int count = clips.Count;
        if (count <= 0) return null;
        int idx = Random.Range(0, count);
        return clips[idx];
    }
}