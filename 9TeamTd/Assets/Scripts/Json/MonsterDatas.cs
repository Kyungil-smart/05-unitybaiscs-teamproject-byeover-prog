using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 김영빈

[Serializable]
public class MonsterDatas
{
    public int id;
    public string name;
    public int level;
    public int maxHp;
    public int attackValue;
    public float defenceValue;
    public string moveType;
    public string enemyRank;
    public float moveSpeed;
}

// MonsterDatas 데이터 리스트
[Serializable]
public class MonsterDataList
{
    public List<MonsterDatas> monsters;
}