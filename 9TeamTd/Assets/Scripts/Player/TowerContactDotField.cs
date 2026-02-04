using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [역할]
/// - "유저가 설치한 타워" 주변/범위에 들어온 몬스터에게
///   DoT(초당 피해) + 슬로우를 '접촉 중'인 동안 지속 적용.
/// 
/// [설계 의도]
/// - 타워 시스템을 안 건드리고, 타워 프리팹에 붙이는 추가 컴포넌트 1개로 해결.
/// - OnTriggerStay는 호출이 잦을 수 있어 "타이머 기반 틱"으로 계산(성능/디버깅 친화).
/// </summary>
[DisallowMultipleComponent]
public sealed class TowerContactDotField : MonoBehaviour
{
    [Header("대상 레이어(몬스터)")]
    [Tooltip("몬스터 레이어만 체크. 비어있으면 레이어 필터를 사용하지 않습니다.")]
    [SerializeField] private LayerMask monsterLayerMask = 0;

    [Header("도트(초당 피해)")]
    [SerializeField, Min(0f)] private float dotDamagePerSecond = 5f;

    [Header("틱 간격(초)")]
    [Tooltip("피해를 매 프레임 주지 않고, 일정 간격으로 나눠서 줌(성능/일관성).")]
    [SerializeField, Min(0.02f)] private float tickInterval = 0.2f;

    [Header("슬로우(이동속도 배수)")]
    [Tooltip("0.5면 이동속도 50%")]
    [SerializeField, Range(0.05f, 1f)] private float slowMultiplier = 0.6f;

    [Header("슬로우 유지시간(초)")]
    [Tooltip("접촉이 끊겨도 잠깐 유지되게(끊김 방지). 0이면 즉시 해제.")]
    [SerializeField, Min(0f)] private float slowHoldSeconds = 0.15f;

    // 몬스터별 마지막 틱 시간 관리(불필요한 연산/중복 피해 방지)
    private readonly Dictionary<int, float> _nextTickTimeByInstanceId = new Dictionary<int, float>(128);

    private void Reset()
    {
        // 실수 방지: 타워에 Collider가 없으면 효과가 불가.
        // (Unity Inspector에서 반드시 Trigger Collider를 켜야 함)
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsMonster(other)) return;

        int id = other.GetInstanceID();
        if (!_nextTickTimeByInstanceId.ContainsKey(id))
            _nextTickTimeByInstanceId.Add(id, 0f);

        // 접촉 시작 즉시 슬로우 걸기
        ApplySlow(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsMonster(other)) return;

        int id = other.GetInstanceID();
        if (!_nextTickTimeByInstanceId.TryGetValue(id, out float nextTime))
        {
            _nextTickTimeByInstanceId[id] = 0f;
            nextTime = 0f;
        }

        // 슬로우는 접촉 중 계속 갱신(끊김 방지)
        ApplySlow(other);

        float now = Time.time;
        if (now < nextTime) return;

        _nextTickTimeByInstanceId[id] = now + tickInterval;

        // 이번 틱에서 줄 피해량 = DPS * tickInterval
        float damageThisTick = dotDamagePerSecond * tickInterval;

        // 데미지 적용(프로젝트마다 HP 스크립트가 다르니, 가장 안전한 방식: 인터페이스/메시지)
        // 1) MonsterHealth 같은 컴포넌트가 있으면 우선 호출
        if (other.TryGetComponent(out MonsterHealthCompat hp))
        {
            hp.ApplyDamage(damageThisTick);
        }
        else
        {
            // 2) 호환용: ReceiveDamage(float) 같은 함수가 있으면 SendMessage로 호출(없으면 무시)
            other.gameObject.SendMessage("ReceiveDamage", damageThisTick, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsMonster(other)) return;

        int id = other.GetInstanceID();
        _nextTickTimeByInstanceId.Remove(id);

        // 슬로우 해제는 몬스터 쪽에서 "유지시간 지나면 자동 해제"하게 처리(아래 스크립트)
        if (other.TryGetComponent(out MonsterSlowReceiver slow))
        {
            slow.RequestSlow(slowMultiplier, 0f); // exit 시 즉시 갱신 안 함(유지시간은 Stay에서 부여)
        }
    }

    private bool IsMonster(Collider other)
    {
        if (monsterLayerMask == 0) return true;
        return (monsterLayerMask.value & (1 << other.gameObject.layer)) != 0;
    }

    private void ApplySlow(Collider other)
    {
        // 몬스터에 슬로우 수신기가 있으면 갱신
        if (other.TryGetComponent(out MonsterSlowReceiver slow))
        {
            slow.RequestSlow(slowMultiplier, slowHoldSeconds);
        }
    }
}

/// <summary>
/// [주의]
/// - 프로젝트마다 몬스터 체력 스크립트 이름이 달라서, "연결용 어댑터"를 최소로 둠.
/// - 실제로는 너희 프로젝트의 MonsterHealth/MonsterStats에 맞춰 이 클래스만 내부를 바꾸면 됨.
/// </summary>
public sealed class MonsterHealthCompat : MonoBehaviour
{
    [Header("임시 HP(프로젝트 HP로 교체 권장)")]
    [SerializeField, Min(1f)] private float maxHp = 20f;
    [SerializeField] private float currentHp;

    private void Awake()
    {
        if (currentHp <= 0f) currentHp = maxHp;
    }

    public void ApplyDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHp -= amount;
        if (currentHp <= 0f)
        {
            // 프로젝트의 몬스터 죽음 처리로 교체 권장
            Destroy(gameObject);
        }
    }
}
