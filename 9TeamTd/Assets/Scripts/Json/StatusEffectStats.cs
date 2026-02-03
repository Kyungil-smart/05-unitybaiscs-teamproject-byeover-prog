using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class StatusEffectStats : MonoBehaviour
{
    // TowerData.cs 참고하여 모두 설정해줌
    [Header("Projectile Status")]
    public int id;
    public string name;




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

        StatusEffectDatas tData = JsonManager.instanceJsonManger.GetStatusEffectData(id);

        if (tData != null)
        {
            SetupValue(tData);
            StatusEffect towerScript = GetComponent<StatusEffect>();

            if (towerScript != null)
            {
                towerScript.InitStats(this);
            }
        }

    }


    public void SetupValue(StatusEffectDatas data)
    {
        if (data == null) return;

        // json과 동일해야 함
        id = data.id;
        name = data.name;


        Debug.Log($"{name}의 능력치 설정 완료");
    }




}



