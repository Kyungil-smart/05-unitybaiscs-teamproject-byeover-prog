using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 김영빈

public class MonsterStats : MonoBehaviour
{
    // MonsterDatas 스크립트 일치시키기
    [Header("Monster Status")]
    public int id;
    public string name;
    public int level;
    public int maxHP;
    public float attackValue;
    public float defenceValue;
    public int Type;
    public string enemyRank;
    public float moveSpeed;


    private void Awake()
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

        MonsterDatas mData = JsonManager.instanceJsonManger.GetMonsterData(id, level);
        // TowerDatas tData = JsonManager.instanceJsonManger.GetTowerData(id, level);

        if (mData != null)
        {
            SetupValue(mData);
            
            
            Monster monsterScript = GetComponent<Monster>();

            if (monsterScript != null)
            {
                monsterScript.GetMonsterStats(this);
            }
        }
    }
    
    public void SetupValue(MonsterDatas data)
    {
        if (data == null) return;

        id = data.id;
        name = data.name;
        level = data.level;
        maxHP = data.maxHP;
        attackValue = data.attackValue;
        defenceValue = data.defenceValue;
        Type = data.Type;
        enemyRank = data.enemyRank;
        moveSpeed = data.moveSpeed;
    }
}
