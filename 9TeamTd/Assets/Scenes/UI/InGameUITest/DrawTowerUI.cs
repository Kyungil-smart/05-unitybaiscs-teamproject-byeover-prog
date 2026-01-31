using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DrawTowerUI : MonoBehaviour
{
    // 테스트 코드입니다

    public void SelectTowerBtn00()
    {
        FindObjectOfType<DrawTower>().targetTower = 0;
    }

    public void SelectTowerBtn01()
    {
        FindObjectOfType<DrawTower>().targetTower = 1;
    }
}
