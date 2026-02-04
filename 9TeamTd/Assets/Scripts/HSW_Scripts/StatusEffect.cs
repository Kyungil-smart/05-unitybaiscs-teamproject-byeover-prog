using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    [SerializeField] private GameObject attacker;
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 moveDirection;

    [SerializeField] private int id;
    [SerializeField] private string effectType;
    [SerializeField] private string effectClass;
    [SerializeField] private string effectRate;
    [SerializeField] private string effectValue;
    [SerializeField] private string effectInterval;
    [SerializeField] private string duration;
    [SerializeField] private string overlapCount;


    public void InitStats(StatusEffectStats stats)
    {




    }
}
