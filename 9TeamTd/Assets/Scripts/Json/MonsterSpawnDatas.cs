using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 김영빈

[Serializable]
public class MonsterSpawnDatas
{
    public int id;
    public float startTime;
    public float endTime;
    public float interval;
}

[Serializable]
public class MonsterSpawnDataList
{
    public List<MonsterSpawnDatas> spawnPattern;
}
