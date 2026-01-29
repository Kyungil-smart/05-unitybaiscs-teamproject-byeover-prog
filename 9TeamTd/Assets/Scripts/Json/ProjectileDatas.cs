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
    public ProjectileSpwanType projectileSpwanType;
    public ProjectileSpacialAbility projectileSpacialAbility;
    public DamageTargetTeamType damageTargetTeamType;
    public ProjectileDamageCategory projectileDamageCategory;
}

// ProjectileDatas 형식의 리스트로 만들어 관리 
[Serializable]
public class ProjectileDataList
{
    public List<ProjectileDatas> projectiles;
}