using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static ProjectileEnumData;
using static TowerEnumData;

// 작성자 : 한성우

public class TowerStats : MonoBehaviour
{
    // 미리 입력을 해야함
    [Header("Key Status")]
    public int id;
    public int level;

    // TowerData.cs 참고하여 모두 설정해줌
    [Header("Auto Status")]
    public string name;
    public TowerType towerType;
    public int maxHP;
    public attackType attackType;
    public int attackValue;
    public float attackRange;
    public int attackProjectileIDs;
    public float attackSpeed;
    public int defenceValue;
    public int towerCost;


    // 시작시 초기화
    private void Start()
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

        TowerDatas tData = JsonManager.instanceJsonManger.GetTowerData(id, level);

        if (tData != null)
        {
            SetupValue(tData);
            Tower towerScript = GetComponent<Tower>();

            if (towerScript != null)
            {
                towerScript.GetStats(this);
            }
        }

    }


    // 호출 받으면 TowerDatas.cs 참고하여 모두 설정해줌
    public void SetupValue(TowerDatas data)
    {
        if (data == null) return;

        // json과 동일해야 함
        id = data.id;
        name = data.name;
        level = data.level;
        towerType = (TowerType)Enum.Parse(typeof(TowerType), data.towerType);
        maxHP = data.maxHP;
        attackType = (attackType)Enum.Parse(typeof(attackType), data.attackType);
        attackValue = data.attackValue;
        attackRange = data.attackRange;
        attackProjectileIDs = data.attackProjectileIDs;
        attackSpeed = data.attackSpeed;
        defenceValue = data.defenceValue;
        towerCost = data.towerCost;


        Debug.Log($"{name}의 능력치 설정 완료");
    }


    // 건설할 타워 금액 알려주는 함수
    public int SetCost(int gId, int gLv)
    {
        TowerDatas tData = JsonManager.instanceJsonManger.GetTowerData(gId, gLv);

        if (tData == null)
        {
            SetupValue(tData);

        }
        
        if (tData.level != gLv)
        {
            Debug.LogError($"{gId}의 {gLv} 레벨 데이터가 없습니다.");
            return 999999;
        }
        
        return tData.towerCost;

    }


    public bool HasNextLevel()
    {
        if (JsonManager.instanceJsonManger == null) return false;

        // 현재 레벨 + 1 한 데이터가 있는지 json 매니저 통해서 판단
        return JsonManager.instanceJsonManger.HasTowerData(id, level + 1);
    }



    // 레벨업할 타워 금액 알려주는 함수
    public int? LevelUpCost()
    {
        // 다음 레벨이 없으면 null 
        if (HasNextLevel() == false)
        {
            Debug.Log($" {name} (ID:{id})는 이미 최대 레벨");
            return null; 
        }

        TowerDatas tData = JsonManager.instanceJsonManger.GetTowerData(id, level + 1);

        Debug.Log($"{tData.towerCost} 필요");

        return tData.towerCost;
    }

    // 타워 레벨 업 함수
    public void LevelUp()
    {
        if (HasNextLevel() == false)
        {
            Debug.Log($" {name} (ID:{id})는 최대 레벨");
            return;
        }


        // 자신의 level 1만 올려 다시 json 요청
        TowerDatas tData = JsonManager.instanceJsonManger.GetTowerData(id, level + 1);

        /*
        if (tData == null)
        {
            return;
            Debug.Log("데이터 확인 불가");
        }
        */

        // 1레벨 올려 스텟 다시 세팅
        SetupValue(tData);
        Tower towerScript = GetComponent<Tower>();

        if (towerScript != null)
        {
            towerScript.GetStats(this);
        }
        Debug.Log("레벨 업 함수 실행 완료");
    }

    
    // 테스트용 레벨업 함수
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P 입력");
            LevelUpCost();
            LevelUp();       
        }
    }
    
}
