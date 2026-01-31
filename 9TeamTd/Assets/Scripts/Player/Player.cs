using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI에 할당하기 위해 플레이어를 임시로 작성했습니다 - 여기다가 플레이어 데이터를 수정하여 작성하길 권장합니다
public class Player : MonoBehaviour
{
    public int atkPower;
    public int atkRange;
    public float atkSpeed;

    public static OP<int> gold = new();

    private void Awake()
    {
        gold.Value = 110;
    }
}