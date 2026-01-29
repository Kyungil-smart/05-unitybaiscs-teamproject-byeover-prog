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
        projectileSpwanType = data.projectileSpwanType;
        projectileSpacialAbility = data.projectileSpacialAbility;
        damageTargetTeamType = data.damageTargetTeamType;
        projectileDamageCategory = data.projectileDamageCategory;


        Debug.Log($"{name}의 능력치 설정 완료");
    }
}
