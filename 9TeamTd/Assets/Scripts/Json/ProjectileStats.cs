using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectileEnumData;

// 작성자 : 한성우

public class ProjectileStats : MonoBehaviour
{
    // TowerData.cs 참고하여 모두 설정해줌
    [Header("Projectile Status")]
    public int id;
    public string name;
    public float moveSpeed;
    public float lifeTime;
    public float damageInterval;
    public ProjectileSpwanType projectileSpwanType;
    public ProjectileSpacialAbility projectileSpacialAbility;
    public DamageTargetTeamType damageTargetTeamType;
    public ProjectileDamageCategory projectileDamageCategory;


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

        ProjectileDatas tData = JsonManager.instanceJsonManger.GetProjectileData(id);

        if (tData != null)
        {
            SetupValue(tData);
            Projectile towerScript = GetComponent<Projectile>();

            if (towerScript != null)
            {
                towerScript.InitStats(this);
            }
        }

    }





    // 호출 받으면 ProjectileDatas.cs 참고하여 모두 설정해줌
    public void SetupValue(ProjectileDatas data)
    {
        if (data == null) return;

        // json과 동일해야 함
        id = data.id;
        name = data.name;
        moveSpeed = data.moveSpeed;
        lifeTime = data.lifeTime;
        damageInterval = data.damageInterval;
        projectileSpwanType = (ProjectileSpwanType)Enum.Parse(typeof(ProjectileSpwanType), data.projectileSpwanType);
        projectileSpacialAbility = (ProjectileSpacialAbility)Enum.Parse(typeof(ProjectileSpacialAbility), data.projectileSpacialAbility);
        damageTargetTeamType = (DamageTargetTeamType)Enum.Parse(typeof(DamageTargetTeamType), data.damageTargetTeamType);
        projectileDamageCategory = (ProjectileDamageCategory)Enum.Parse(typeof(ProjectileDamageCategory), data.projectileDamageCategory);


        Debug.Log($"{name}의 능력치 설정 완료");
    }
}
