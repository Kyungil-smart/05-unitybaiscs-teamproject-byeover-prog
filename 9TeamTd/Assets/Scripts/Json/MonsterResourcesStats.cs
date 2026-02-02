using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterResourcesStats : MonoBehaviour
{
    [Header("Monster Resources Stats")]
    public int id;
    public int gold;
    public string DropItemId;
    public float DropProp;

    public void SetUpValue(MonsterResourcesDatas resourcesData)
    {
        if (resourcesData == null) return;
        
        id = resourcesData.id;
        gold = resourcesData.gold;
        DropItemId = resourcesData.DropItemId;
        DropProp = resourcesData.DropProp;
    }
}
