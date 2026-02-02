using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static ProjectileEnumData;
using static StatusEffectEnumData;

public class StatusEffectStats : MonoBehaviour
{
    // TowerData.cs 참고하여 모두 설정해줌
    [Header("Projectile Status")]
    public int id;
    public EffectType effectType;
    public EffectClass effectClass;
    public string effectRate;
    public string effectValue;
    public string effectInterval;
    public string duration;
    public string overlapCount;




    // 시작시 초기화
    private void Start()
    {
        Init();
    }


    public void Init()
    {
        // 버그 확인용
        if (JsonManager.instanceJsonManger == null)
        {
            Debug.LogError("JsonManager 없음");
            return;
        }

        StatusEffectDatas tData = JsonManager.instanceJsonManger.GetStatusEffectData(id);

        if (tData != null)
        {
            SetupValue(tData);
            StatusEffect towerScript = GetComponent<StatusEffect>();

            if (towerScript != null)
            {
                towerScript.InitStats(this);
            }
        }

    }


    public void SetupValue(StatusEffectDatas data)
    {
        if (data == null) return;

        // json과 동일해야 함
        id = data.id;
        effectType = (EffectType)Enum.Parse(typeof(EffectType), data.effectType);
        effectClass = (EffectClass)Enum.Parse(typeof(EffectClass), data.effectClass);
        effectRate = data.effectRate;
        effectValue = data.effectValue;
        effectInterval = data.effectInterval;
        duration = data.duration;
        overlapCount = data.overlapCount;


        Debug.Log($"{name}의 능력치 설정 완료");
    }




}



