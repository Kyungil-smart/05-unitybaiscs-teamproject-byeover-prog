using System;
using System.Collections;
using System.Collections.Generic; // 데이터에 리스트 사용시 필요
using UnityEngine;
using static ProjectileEnumData;

// 작성자 : 한성우

[Serializable]
public class ProjectileDatas // .json 파일과 이름과 겹치면 안 됨
{
    public int id;
    public string name;
    public float moveSpeed;
    public float lifeTime;
    public float damageInterval;
    public string projectileSpwanType;
    public string projectileSpacialAbility;
    public string damageTargetTeamType;
    public string projectileDamageCategory;

    public int effectRate;   // 이 아래는 상태 효과를 위해 추가
    public float effectValue;
    public float effectInterval;
    public float duration;
    public int overlapCount;
}

// ProjectileDatas 형식의 리스트로 만들어 관리 
[Serializable]
public class ProjectileDataList
{
    public List<ProjectileDatas> projectiles;
}