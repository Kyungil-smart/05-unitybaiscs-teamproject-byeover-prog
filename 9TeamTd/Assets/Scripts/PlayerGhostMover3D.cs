using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerGhostMover3D : MonoBehaviour
{
    [Header("이동")]
    [SerializeField, Min(0f)] private float move_speed = 6f;

    [Header("능력치(인스펙터에서 조절)")]
    [SerializeField, Min(0)] private int max_hp = 10;
    [SerializeField, Min(0)] private int current_hp = 10;

    [Header("맵 경계(맵 밖으로 못 나가게)")]
    [Tooltip("GridSystem이 씬에 있으면 자동으로 범위를 가져옵니다. 비어있으면 직접 설정한 경계를 사용합니다.")]
    [SerializeField] private GridSystem grid_system = null;

    [Tooltip("grid_system이 없을 때 사용하는 XZ 최소/최대 경계(월드 좌표)")]
    [SerializeField] private Vector2 fallback_min_xz = new Vector2(0f, 0f);
    [SerializeField] private Vector2 fallback_max_xz = new Vector2(30f, 20f);

    [Tooltip("맵 경계 안쪽으로 살짝 여유를 줍니다(플레이어 반경 같은 느낌).")]
    [SerializeField, Min(0f)] private float clamp_padding = 0.25f;

    [Header("디버그")]
    [SerializeField] private bool verbose_logs = false;

    private Rigidbody _rb;
    private Vector3 _move_dir;
    private Vector2 _min_xz;
    private Vector2 _max_xz;

    // ---- 외부에서 읽기/설정 가능(요청사항: 능력치 public/serialize 확인용) ----
    public int MaxHp => max_hp;
    public int CurrentHp => current_hp;

    private void Awake()
    {
        // Rigidbody 필수: 이동 안정성 + 입력 먹통(리짓바디 없음) 이슈 재발 방지
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
        }

        // 유령 컨셉: 물리 영향 최소화(충돌/중력/회전 영향 제거)
        _rb.useGravity = false;
        _rb.isKinematic = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 현재 HP가 Max를 넘지 않게 보정
        if (current_hp > max_hp) current_hp = max_hp;
        if (current_hp < 0) current_hp = 0;

        RebuildClampBounds();
    }

    private void OnValidate()
    {
        if (max_hp < 0) max_hp = 0;
        if (current_hp < 0) current_hp = 0;
        if (current_hp > max_hp) current_hp = max_hp;

        if (clamp_padding < 0f) clamp_padding = 0f;
    }

    private void Start()
    {
        // Start에서 한 번 더(씬 로딩/참조 연결 순서 차이 방어)
        RebuildClampBounds();
    }

    private void Update()
    {
        // 입력: 프로젝트가 Input System / Legacy 섞여서 꼬인 적이 있으니
        // 여기선 "Legacy(Input.GetAxisRaw)"로 고정(가장 덜 터짐)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 dir = new Vector3(x, 0f, z);
        if (dir.sqrMagnitude > 1f) dir.Normalize();
        _move_dir = dir;

        if (verbose_logs)
        {
            Debug.Log($"[PlayerMover] input=({x},{z}) dir={_move_dir}");
        }

        // GridSystem이 런타임에 바뀌거나 맵 크기 바뀌는 경우를 대비해
        // 필요하면 여기서 주기적으로 RebuildClampBounds()를 호출하게 확장 가능.
    }

    private void FixedUpdate()
    {
        // 물리 프레임에서 이동(리짓바디 기반)
        Vector3 target_pos = _rb.position + (_move_dir * (move_speed * Time.fixedDeltaTime));
        target_pos = ClampToMap(target_pos);

        _rb.MovePosition(target_pos);
    }

    // ----------------- 맵 경계 계산/클램프 -----------------
    private void RebuildClampBounds()
    {
        if (grid_system != null)
        {
            // GridSystem 기준: (0,0) ~ (width,height) 셀을 월드로 펼친 영역으로 가정
            // - 기존 GridSystem의 cellSize, width/height가 private라도
            //   GridSystem 쪽에 "월드 경계 반환 함수"가 이미 있을 수 있어서 우선 시도하고,
            //   없으면 fallback으로 간다.

            // 1) GridSystem에 공개 함수가 있다면 그걸 쓰는 게 정답인데,
            //    지금은 프로젝트마다 달라서 안전하게 "fallback + 명시 설정"도 같이 둠.
            // 2) 그래서 여기선 grid_system의 Transform을 기준으로 fallback을 재해석한다.
            //    (즉, grid_system이 있는 경우: fallback_min/max를 "로컬 오프셋"처럼 활용)

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

    // ----------------- (옵션) HP 제어 -----------------
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
