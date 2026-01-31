using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerEnumData : MonoBehaviour
{
    public enum TowerType
    {
        Default,    // 사용 안하는 디폴트 값 (버그 확인용)
        None,
        Base,
        Tower,
    }

    public enum attackType
    {
        Default,    // 사용 안하는 디폴트 값 (버그 확인용)
        None,
        Always, // 적 없어도 쿨타임만 맞으면 공격
        Target,    // 타겟이 있을 시 공격
    }
}
