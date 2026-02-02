using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StatusEffectDatas // .json 파일과 이름과 겹치면 안 됨
{
    public int id;
    public string effectType;
    public string effectClass;
    public string effectRate;
    public string effectValue;
    public string effectInterval;
    public string duration;
    public string overlapCount;
        


}


// ProjectileDatas 형식의 리스트로 만들어 관리 
[Serializable]
public class StatusEffectDataList
{
    public List<StatusEffectDatas> statusEffects;
}