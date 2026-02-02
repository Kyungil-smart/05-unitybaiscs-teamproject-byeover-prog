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
    public int lastOpenStageNum = 1;    // 가장 마지막에 오픈된 스테이지 (클리어시 자동 오픈)
    public int MaxStageNum = 3; // 만들어진 최대 스테이지
    public int outGameGem = 100;    // 아웃 게임 재화
    public List<OpenBases> GetBases;    //해금된 기지 리스트, 0 해금 안됨, 1 ~ 2, json 에서 타워 코스트랑 같은 칼럼 사용하지만, 재화만 outGameGem으로
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
