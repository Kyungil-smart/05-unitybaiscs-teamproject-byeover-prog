using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerGhostMover3D : MonoBehaviour
{
    // =========================
    // 이동
    // =========================
    [Header("이동")]
    [SerializeField, Min(0f)] private float move_speed = 6f;

    // =========================
    // HP(지금은 테스트용)
    // =========================
    [Header("능력치(인스펙터에서 조절)")]
    [SerializeField, Min(0)] private int max_hp = 10;
    [SerializeField, Min(0)] private int current_hp = 10;

    // =========================
    // 전투(인스펙터 슬롯)
    // =========================
    [Header("전투(인스펙터 슬롯)")]
    [SerializeField, Min(0)] private int attack_damage = 10;       // 공격력
    [SerializeField, Min(0f)] private float attack_speed = 1f;     // 공격속도(초당 공격 횟수 느낌)
    [SerializeField, Min(0f)] private float attack_range = 6f;     // 사거리
    [SerializeField] private GameObject projectile_prefab = null;  // 투사체 오브젝트(프리팹)

    // =========================
    // [요청] 드랍(인스펙터 슬롯)
    // - itemId(string)을 int로 바꿔서
    //   아래 item_prefabs[index] 프리팹을 생성하는 구조
    // =========================
    [Header("드랍(인스펙터 슬롯)")]
    [Tooltip("ID(int) = 리스트 인덱스. 예) itemId='0' -> item_prefabs[0]")]
    [SerializeField] private List<GameObject> item_prefabs = new List<GameObject>();

    [Tooltip("드랍 위치에서 위로 띄우는 높이(기본 1.5)")]
    [SerializeField] private float drop_height_offset = 1.5f;

    // =========================
    // 맵 경계(맵 밖으로 못 나가게)
    // =========================
    [Header("맵 경계(맵 밖으로 못 나가게)")]
    [Tooltip("GridSystem이 씬에 있으면 자동으로 범위를 가져옵니다. 비어있으면 직접 설정한 경계를 사용합니다.")]
    [SerializeField] private GridSystem grid_system = null;

    [Tooltip("grid_system이 없을 때 사용하는 XZ 최소/최대 경계(월드 좌표)")]
    [SerializeField] private Vector2 fallback_min_xz = new Vector2(0f, 0f);
    [SerializeField] private Vector2 fallback_max_xz = new Vector2(30f, 20f);

    [Tooltip("맵 경계 안쪽으로 살짝 여유를 줍니다(플레이어 반경 같은 느낌).")]
    [SerializeField, Min(0f)] private float clamp_padding = 0.25f;

    // =========================
    // 디버그
    // =========================
    [Header("디버그")]
    [SerializeField] private bool verbose_logs = false;

    // =========================
    // 내부 변수
    // =========================
    private Rigidbody _rb;
    private Vector3 _move_dir;
    private Vector2 _min_xz;
    private Vector2 _max_xz;

    // =========================
    // 외부에서 읽기(필요할 때 사용)
    // =========================
    public int MaxHp => max_hp;
    public int CurrentHp => current_hp;

    public int AttackDamage => attack_damage;
    public float AttackSpeed => attack_speed;
    public float AttackRange => attack_range;
    public GameObject ProjectilePrefab => projectile_prefab;

    // =========================
    // 생명주기
    // =========================
    private void Awake()
    {
        // [필수] Rigidbody 없으면 추가 (입력 먹통/이동 불안정 방지)
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();

        // [유령 컨셉] 물리 영향 최소화
        _rb.useGravity = false;
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        // HP 보정
        if (current_hp > max_hp) current_hp = max_hp;
        if (current_hp < 0) current_hp = 0;

        RebuildClampBounds();
    }

    private void OnValidate()
    {
        // HP 방어
        if (max_hp < 0) max_hp = 0;
        if (current_hp < 0) current_hp = 0;
        if (current_hp > max_hp) current_hp = max_hp;

        // 전투 값 방어
        if (attack_damage < 0) attack_damage = 0;
        if (attack_speed < 0f) attack_speed = 0f;
        if (attack_range < 0f) attack_range = 0f;

        // 드랍 값 방어
        if (drop_height_offset < 0f) drop_height_offset = 0f;

        // 경계 패딩 방어
        if (clamp_padding < 0f) clamp_padding = 0f;
    }

    private void Start()
    {
        // 씬 로딩/참조 순서 방어
        RebuildClampBounds();
    }

    private void Update()
    {
        // [입력] Legacy(Input.GetAxisRaw) 고정(가장 덜 터짐)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(x, 0f, z);
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        _move_dir = dir;

        if (verbose_logs)
            Debug.Log($"[PlayerMover] input=({x},{z}) dir={_move_dir}");
    }

    private void FixedUpdate()
    {
        // [이동] Rigidbody 기반 이동
        Vector3 target_pos = _rb.position + (_move_dir * (move_speed * Time.fixedDeltaTime));
        target_pos = ClampToMap(target_pos);
        _rb.MovePosition(target_pos);
    }

    // =========================
    // 맵 경계 계산/클램프
    // =========================
    private void RebuildClampBounds()
    {
        if (grid_system != null)
        {
            // GridSystem 원점 기준으로 fallback 범위를 오프셋 처리
            Vector3 origin = grid_system.transform.position;
            _min_xz = new Vector2(origin.x + fallback_min_xz.x, origin.z + fallback_min_xz.y);
            _max_xz = new Vector2(origin.x + fallback_max_xz.x, origin.z + fallback_max_xz.y);

            if (verbose_logs)
                Debug.Log($"[PlayerMover] bounds from GridSystem origin. min={_min_xz} max={_max_xz}");
        }
        else
        {
            _min_xz = fallback_min_xz;
            _max_xz = fallback_max_xz;

            if (verbose_logs)
                Debug.Log($"[PlayerMover] bounds fallback. min={_min_xz} max={_max_xz}");
        }
    }

    private Vector3 ClampToMap(Vector3 pos)
    {
        float min_x = _min_xz.x + clamp_padding;
        float max_x = _max_xz.x - clamp_padding;
        float min_z = _min_xz.y + clamp_padding;
        float max_z = _max_xz.y - clamp_padding;

        pos.x = Mathf.Clamp(pos.x, min_x, max_x);
        pos.z = Mathf.Clamp(pos.z, min_z, max_z);
        return pos;
    }

    // =========================
    // [요청] 아이템 드랍 시스템
    // - 몬스터 사망 시 외부에서 호출하는 형태가 정석
    //   예) player.TryDropItem("0", 0.25f, monsterPos);
    // =========================

    /// <summary>
    /// [역할]
    /// - 확률(prob)을 통과하면 itemId에 해당하는 프리팹을 드랍 위치에 생성한다.
    /// [주의]
    /// - itemId는 "문자열"이지만 내부에서 int로 변환하여 item_prefabs 인덱스로 사용한다.
    /// </summary>
    public void TryDropItem(string itemId, float prob, Vector3 spawnPos)
    {
        // ID가 비었거나 확률이 0 이하이면 종료
        if (string.IsNullOrEmpty(itemId) || prob <= 0f) return;

        // 확률 체크(Random.value: 0~1)
        if (Random.value <= prob)
        {
            SpawnItem(itemId, spawnPos);
        }
        else
        {
            if (verbose_logs)
                Debug.Log($"[Drop] fail(prob) id={itemId} prob={prob}");
        }
    }

    /// <summary>
    /// [역할]
    /// - itemId(string) -> int 변환
    /// - item_prefabs[index]를 Instantiate
    /// </summary>
    private void SpawnItem(string itemId, Vector3 dropPos)
    {
        // string ID -> int 변환
        if (!int.TryParse(itemId, out int index))
        {
            Debug.LogError($"아이템 ID 변환 실패: {itemId}");
            return;
        }

        // 리스트 범위 체크
        if (index < 0 || index >= item_prefabs.Count)
        {
            Debug.LogWarning($"아이템 프리팹이 리스트에 없습니다. ID: {index}");
            return;
        }

        GameObject prefab = item_prefabs[index];
        if (prefab == null)
        {
            Debug.LogWarning($"아이템 프리팹이 비어있습니다. ID: {index}");
            return;
        }

        // 아이템 생성 (위치 살짝 위로 보정)
        Vector3 finalPos = dropPos + new Vector3(0f, drop_height_offset, 0f);
        Instantiate(prefab, finalPos, Quaternion.identity);

        if (verbose_logs)
            Debug.Log($"아이템 드랍 성공 (ID: {index}) pos={finalPos}");
    }

    // =========================
    // (옵션) HP 제어
    // =========================
    public void SetHp(int new_hp)
    {
        current_hp = Mathf.Clamp(new_hp, 0, max_hp);
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;
        current_hp -= dmg;
        if (current_hp < 0) current_hp = 0;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        current_hp += amount;
        if (current_hp > max_hp) current_hp = max_hp;
    }
}