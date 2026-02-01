using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 한성우


public static class DamageCalculator
{
    public static int CalculatingDamage(int atkValue, float dmgRatio, int dfnValue)
    {
        int finalDamage = Mathf.Clamp((int)(dmgRatio * (atkValue - dfnValue)), 0, int.MaxValue); // 피해량이 0 ~ int 최대값 사이가 되도록 처리
        return finalDamage;
    }
}
