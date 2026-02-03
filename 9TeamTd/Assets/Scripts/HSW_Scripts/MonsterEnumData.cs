using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterEnumData : MonoBehaviour
{
    public enum MoveType
    {
        Default,    // 사용 안하는 디폴트 값 (버그 확인용)
        Ground,
        Flying,
    }

    public enum EnemyRank
    {
        Default,    // 사용 안하는 디폴트 값 (버그 확인용)
        Normal,
    }
}
