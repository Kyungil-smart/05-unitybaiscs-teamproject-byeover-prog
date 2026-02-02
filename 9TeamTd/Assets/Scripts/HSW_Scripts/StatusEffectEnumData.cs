using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectEnumData : MonoBehaviour
{
    public enum EffectType
    {
        Default,    // 사용 안하는 디폴트 값 (버그 확인용)
        None,

        Stun,
        Knockback,

        Freeze,

        Burn,

    }

    public enum EffectClass
    {
        Default,    // 사용 안하는 디폴트 값 (버그 확인용)
        None,
        buff,
        CC,
        Debuff,
        Dot
    }
}
