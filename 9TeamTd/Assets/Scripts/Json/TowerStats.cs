using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 한성우

public class TowerStats : MonoBehaviour
{
    // TowerData.cs 참고하여 모두 설정해줌
    [Header("Tower Status")]
    public int id;  // 식별자
    public string name;
    public int level;
    public int maxHP;
    public int attackValue;
    public float attackRange;
    public float attackSpeed;
    public int towerCost;


    // 호출 받으면 TowerDatas.cs 참고하여 모두 설정해줌
    public void SetupValue(TowerDatas data)
    {
        if (data == null) return;

        // json과 동일해야 함
        id = data.id;
        name = data.name;
        level = data.level;
        maxHP = data.maxHP;
        attackValue = data.attackValue;
        attackRange = data.attackRange;
        attackSpeed = data.attackSpeed;
        towerCost = data.towerCost;


        Debug.Log($"{name}의 능력치 설정 완료");
    }
}
