using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class JsonManager_02 : MonoBehaviour
{
    [Header("정의 데이터 로드(뼈대)")]
    [SerializeField] private bool verbose_logs = true;

    public void Init()
    {
        if (verbose_logs) Debug.Log("[JsonManager] Init", this);
    }

    public IEnumerator LoadTablesRoutine()
    {
        // TODO: 나중에 Resources/StreamingAssets/persistent 등에서
        // 타워/몬스터/웨이브 정의 JSON 로드 코드가 들어올 자리
        yield return null;

        if (verbose_logs) Debug.Log("[JsonManager] Load tables ok (stub)", this);
    }
}
