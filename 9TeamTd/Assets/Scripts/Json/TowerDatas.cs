using System;
using System.Collections.Generic; // 데이터에 리스트 사용시 필요
using UnityEngine;
using static TowerEnumData;

// 작성자 : 한성우


[Serializable]
public class TowerDatas // .json 파일과 이름과 겹치면 안 됨
{
    public int id;
    public string name;
    public string desc;
    public int level;
    public string towerType;
    public int maxHP;
    public string attackType;
    public int attackValue;
    public float attackRange;
    public int attackProjectileIDs;
    public float attackSpeed;
    public int defenceValue;
    public int towerCost;


}

// TowerDatas 형식의 리스트로 만들어 관리 
[Serializable]
public class TowerDataList
{
    public List<TowerDatas> towers;
}
