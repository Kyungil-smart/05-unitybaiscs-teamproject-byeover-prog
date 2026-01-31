using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 개별 타워 업셀 버튼에 붙이는 스크립트 - 각 버튼마다 targetID 할당
public class TwUpSellBtnUI : MonoBehaviour
{
    [Header("타워 정보")]
    [SerializeField] int targetID;
    [SerializeField] int targetLevel;

    [Header("UI 연결")]
    [SerializeField] TowerToUpSellUI towerToUpSellUI;
}
