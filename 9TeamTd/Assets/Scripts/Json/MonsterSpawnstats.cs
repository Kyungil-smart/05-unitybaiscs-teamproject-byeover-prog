using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 김영빈

public class MonsterSpawnstats : MonoBehaviour
{
    [Header("Monster Spawn Stats")]
    public int id;
    public float startTime;
    public float endTime;
    public float interval;

    public void SetUpValue(MonsterSpawnDatas spawnData)
    {
        if (spawnData == null) return;
        
        id = spawnData.id;
        startTime = spawnData.startTime;
        endTime = spawnData.endTime;
        interval = spawnData.interval;
    }
}
