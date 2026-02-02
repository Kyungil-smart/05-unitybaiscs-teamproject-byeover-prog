using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OpenBases
{
    public int id;
    public int level;
}

public class SaveData
{
    public int lastOpenStageNum = 1;
    public int MaxStageNum = 3;
    public int outGameGem = 100;
    public List<OpenBases> GetBases;
}

public class SaveManager : MonoBehaviour
{
    public SaveData nowSave = new SaveData();

    [HideInInspector] public int nowSlot;

    public static SaveManager instance;


    private void Awake()
    {
        #region 싱글톤
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion

        nowSlot = 0; // 현재 슬롯 0번만 명시적으로 사용하고 있음 (나중에 슬롯 추가하면 변경)
    }

    public string GetPath(int slotNum)
    {
        // 저장 위치 : \Users\(본인 계정명)\AppData\LocalLow\DefaultCompany\9TeamTd
        return Path.Combine(Application.persistentDataPath, $"save_{slotNum}.json");
    }

    // 스테이지 패배시 사용
    public void SaveData(int _outGameGem)
    {
        nowSave.outGameGem += _outGameGem;

        string json = JsonUtility.ToJson(nowSave);
        File.WriteAllText(GetPath(nowSlot), json);
    }

    // 스테이지 클리어 시 사용
    public void SaveData(int number, int _outGameGem)
    {
        if (nowSave.lastOpenStageNum < nowSave.MaxStageNum) nowSave.lastOpenStageNum += number;
        else nowSave.lastOpenStageNum = nowSave.MaxStageNum;

        nowSave.outGameGem += _outGameGem;
        // 해금된 기지 추가 방법 추가 필요

        string json = JsonUtility.ToJson(nowSave);
        File.WriteAllText(GetPath(nowSlot), json);
    }

    // 기지 해금 시 사용
    public void SaveData(int _lastOpenStageNum, int _outGameGem, OpenBases _openBases)
    {
        
        nowSave.outGameGem += _outGameGem;

        string json = JsonUtility.ToJson(nowSave);
        File.WriteAllText(GetPath(nowSlot), json);
    }

    public SaveData LoadData()
    {
        string json = File.ReadAllText(GetPath(nowSlot));
        JsonUtility.FromJsonOverwrite(json, nowSave);

        return nowSave;
    }

    public bool LoadDataForPreview(int slotNum)
    {
        if (!File.Exists(GetPath(slotNum)))
        {
            return false;
        }
        return true;
    }
}
