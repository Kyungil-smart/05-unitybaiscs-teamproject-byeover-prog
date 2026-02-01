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
    public string moveType;
    public string enemyRank;
    public float moveSpeed;
    
    public void SetupValue(MonsterDatas data)
    {
        if (data == null) return;

        id = data.id;
        name = data.name;
        level = data.level;
        maxHP = data.maxHP;
        attackValue = data.attackValue;
        defenceValue = data.defenceValue;
        moveType = data.moveType;
        enemyRank = data.enemyRank;
        moveSpeed = data.moveSpeed;
    }
}
