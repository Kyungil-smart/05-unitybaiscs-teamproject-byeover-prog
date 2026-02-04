using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterResourcesDatas
{
    public int id;
    public int gold;
    public string DropItemId;
    public float DropProp;
}

[Serializable]
public class MonsterResourcesDataList
{
    public List<MonsterResourcesDatas> resource;
}
