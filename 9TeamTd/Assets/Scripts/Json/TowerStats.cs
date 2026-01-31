using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TowerEnumData;

// 작성자 : 한성우

public class TowerStats : MonoBehaviour
{
    // TowerData.cs 참고하여 모두 설정해줌
    [Header("Tower Status")]
    public int id;
    public string name;
    public int level;
    public TowerType towerType;
    public int maxHP;
    public attackType attackType;
    public int attackValue;
    public float attackRange;
    public int attackProjectileIDs;
    public float attackSpeed;
    public int defenceValue;
    public int towerCost;


    // 호출 받으면 TowerDatas.cs 참고하여 모두 설정해줌
    public void SetupValue(TowerDatas data)
    {
        if (data == null) return;

        // json과 동일해야 함
        id = data.id;
        name = data.name;
        level = data.level;
        towerType = data.towerType;
        maxHP = data.maxHP;
        attackType = data.attackType;
        attackValue = data.attackValue;
        attackRange = data.attackRange;
        attackProjectileIDs = data.attackProjectileIDs;
        attackSpeed = data.attackSpeed;
        defenceValue = data.defenceValue;
        towerCost = data.towerCost;


        Debug.Log($"{name}의 능력치 설정 완료");
    }
}
