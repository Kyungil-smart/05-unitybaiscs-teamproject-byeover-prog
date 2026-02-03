using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterHpUI : MonoBehaviour
{
    Slider hpSlider;
    Monster monster;
    MonsterStats stats;

    private void Awake()
    {
        hpSlider = GetComponentInChildren<Slider>();
        monster = GetComponent<Monster>();
        stats = GetComponent<MonsterStats>();
    }

    private void OnEnable()
    {
        monster.currentHp.OnValueChanged += UpdateHpSlider;
    }

    private void OnDisable()
    {
        monster.currentHp.OnValueChanged -= UpdateHpSlider;
    }

    void UpdateHpSlider(int value)
    {
        hpSlider.value = (float)value / stats.maxHP;
    }
}
