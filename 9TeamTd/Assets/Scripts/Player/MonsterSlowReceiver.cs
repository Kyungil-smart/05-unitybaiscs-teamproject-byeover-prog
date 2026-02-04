using UnityEngine;

/// <summary>
/// [역할]
/// - 타워 접촉 등 외부 효과(슬로우)를 받는 수신기.
/// - 이동 스크립트는 "GetSpeedMultiplier()"를 곱해서 최종 속도를 계산하면 끝.
/// 
/// [설계 의도]
/// - DoT 쪽은 타워에서 처리, 슬로우의 "유지/해제"는 몬스터가 책임.
/// - 접촉이 잠깐 끊겨도 느려짐이 깜빡이지 않게 holdSeconds 지원.
/// </summary>
[DisallowMultipleComponent]
public sealed class MonsterSlowReceiver : MonoBehaviour
{
    private float _currentMultiplier = 1f;
    private float _expireTime = 0f;

    /// <summary>
    /// multiplier: 0.6이면 60% 속도
    /// holdSeconds: 현재 시간 + holdSeconds 까지 유지
    /// </summary>
    public void RequestSlow(float multiplier, float holdSeconds)
    {
        if (multiplier <= 0f) multiplier = 0.01f;
        if (multiplier > 1f) multiplier = 1f;

        // 여러 슬로우가 들어오면 "더 느린 값(더 작은 배수)"이 우선
        if (multiplier < _currentMultiplier)
            _currentMultiplier = multiplier;

        float t = Time.time + Mathf.Max(0f, holdSeconds);
        if (t > _expireTime) _expireTime = t;
    }

    public float GetSpeedMultiplier()
    {
        // 시간이 지나면 자동 복구
        if (Time.time > _expireTime)
        {
            _currentMultiplier = 1f;
            _expireTime = 0f;
        }
        return _currentMultiplier;
    }
}
