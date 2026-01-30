using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using static TowerStats;
using static UnityEngine.GraphicsBuffer;

// 작성자 : 한성우

public enum JsonType
{
    Default,
    None,
    SceneData,  // 현재 테이블 없음
    BaseData,   // 현재 테이블 없음
    TowerData,
    MonsterData,
    ProjectileData,
    StatusEffectData,   // 현재 테이블 없음
}

// https://3dperson1.tistory.com/117 스크립트 기반 -> 추후 수정 예정
public class JsonManager : MonoBehaviour
{
    public static JsonManager instanceJsonManger { get; private set; }

    [Header("대상의 정보")]
    [SerializeField] private int targetID = 0;
    [SerializeField] private int targetLevel = -1;

    [Header("파일 유형 및 경로")]
    [SerializeField] private JsonType jsonType = JsonType.None;
    [SerializeField] string dataFilePath = "";  // Json 파일 경로


    // 테이블 추가될 때마다 업데이트 필요
    private TowerDataList _towerData;   // TowerStats 의 데이터를 불러오면 됨
    private MonsterDataList _monsterData;
    private ProjectileDataList _projectileData;   // TowerStats 의 데이터를 불러오면 됨


    private void Awake()
    {
        // 이 매니저가 2개 이상 있으면 1개는 삭제
        #region 싱글톤
        if (instanceJsonManger == null)
        {
            instanceJsonManger = this;
            DontDestroyOnLoad(gameObject);
            LoadJsonFiles(); // 파일 불러오기
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion
    }


    // 테이블 추가될 때마다 업데이트 필요
    private void LoadJsonFiles()
    {
        LoadTowerData("Datas/TowerDataExample");
        LoadMonsterData("Datas/MonsterData");
        LoadProjectileData("Datas/ProjectileData");

        /*
        TextAsset jsonDataFile = Resources.Load<TextAsset>(dataFilePath);    // TextAsset(텍스트 파일 형식) 으로 리소스 폴더 하위 경로에서 TowerData 파일을 불러옴//TextAsset jsonDataFile = Resources.Load<TextAsset>("Datas/TowerData");    // TextAsset(텍스트 파일 형식) 으로 리소스 폴더 하위 경로에서 TowerData 파일을 불러옴
        if (jsonDataFile != null)
        {
            _towerData = JsonUtility.FromJson<TowerDataList>(jsonDataFile.text);    // JsonUtility 활용해 TowerDataList 역직렬화
        }
        else
        {
            Debug.LogError("파일 없음");
        }
        */
    }


    // 테이블 추가될 때마다 업데이트 필요
    private void LoadTowerData(string dataFilePath)
    {
        TextAsset jsonDataFile = Resources.Load<TextAsset>(dataFilePath);    // TextAsset(텍스트 파일 형식) 으로 리소스 폴더 하위 경로에서 TowerData 파일을 불러옴//TextAsset jsonDataFile = Resources.Load<TextAsset>("Datas/TowerData");    // TextAsset(텍스트 파일 형식) 으로 리소스 폴더 하위 경로에서 TowerData 파일을 불러옴
        if (jsonDataFile != null)
        {
            _towerData = JsonUtility.FromJson<TowerDataList>(jsonDataFile.text);    // JsonUtility 활용해 TowerDataList 역직렬화
        }
        else
        {
            Debug.LogError($"파일 없음: {dataFilePath}");
        }
    }


    private void LoadMonsterData(string dataFilePath)
    {
        TextAsset jsonDataFile = Resources.Load<TextAsset>(dataFilePath);
        if (jsonDataFile != null)
        {
            _monsterData = JsonUtility.FromJson<MonsterDataList>(jsonDataFile.text);
        }
        else
        {
            Debug.LogError($"파일 없음: {dataFilePath}");
        }
    }


    private void LoadProjectileData(string dataFilePath)
    {
        TextAsset jsonDataFile = Resources.Load<TextAsset>(dataFilePath);
        if (jsonDataFile != null)
        {
            _projectileData = JsonUtility.FromJson<ProjectileDataList>(jsonDataFile.text);
        }
        else
        {
            Debug.LogError($"파일 없음: {dataFilePath}");
        }
    }









    // 각 데이터 별로 찾아서 리턴까지 하도록 기능 수정

    public TowerDatas GetTowerData(int id, int level)
    {
        if (_towerData == null) return null; // 타워 데이터에 없으면 리턴

        // TowerDataList에서 해당 레벨의 ID값의 캐릭터 찾기
        TowerDatas foundData = _towerData.towers.Find(t => t.id == id && t.level == level);

        if (foundData == null)
        {
            Debug.LogError($"ID: {id}, Level: {level} 에 해당하는 타워 데이터 없음");
        }

        return foundData; // 찾은 데이터를 호출한 곳으로 돌려줌
    }

    public MonsterDatas GetMonsterData(int id, int level)
    {
        if (_monsterData == null) return null;

        MonsterDatas foundData = _monsterData.monsters.Find(t => t.id == id && t.level == level);

        if (foundData == null)
        {
            Debug.LogError($"ID: {id}, Level: {level} 에 해당하는 몬스터 데이터 없음");
        }

        return foundData;
    }



    private ProjectileDatas GetProjectileData(int id)
    {
        if (_projectileData == null) return null;

        ProjectileDatas foundData = _projectileData.projectiles.Find(t => t.id == id);

        if (foundData == null)
        {
            Debug.LogWarning($"ID {id}에 해당하는 데이터 없음");
        }

        return foundData;
    }


    /*
    // 이 함수를 통해 불러옴
    public void GetJsonRequest(JsonType type, string path, int id)
    {
        // 불러오기 전에 데이터 변경
        jsonType = type;
        dataFilePath = path;
        targetID = id;

        // 데이터를 분류 및 불러오기
        switch (jsonType)
        {
            case JsonType.None: break;
            case JsonType.SceneData: break;
            case JsonType.ProjectileData:
                ChangeProjectileID(targetID);
                break;
            default: break;
        }
    }
    public void GetJsonRequest(JsonType type, string path, int id, int lv)
    {
        // 불러오기 전에 데이터 변경
        jsonType = type;
        dataFilePath = path;
        targetID = id;
        targetLevel = lv;

        // 데이터를 분류 및 불러오기
        switch (jsonType)
        {
            case JsonType.None: break;
            case JsonType.BaseData: break;
            case JsonType.TowerData:
                ChangeTowerID(targetID, targetLevel);
                break;
            case JsonType.MonsterData: break;
            case JsonType.StatusEffectData: break;
            default: break;
        }
    }
    


    

    // 원하는 ID와 레벨을 받아서 불러오는 용도
    // 타워, 몬스터, 기지 등에서 사용할 예정
    private void ChangeTowerID(int id, int level)
    {
        if (_towerData == null) return; // 타워 데이터에 없으면 리턴

        // TowerDataList에서 해당 레벨의 ID값의 캐릭터 찾기
        TowerDatas foundData = _towerData.towers.Find(t => t.id == id && t.level == level);

        if (foundData != null)
        {
            TowerStats tower = FindFirstObjectByType<TowerStats>(); // 현재 씬에서 배치된 오브젝트중 TowerStats 붙은 오브젝트 1개만 찾기
            if (tower != null)
            {
                tower.SetupValue(foundData); // TowerStats 에서 받은 능력치로 설정
            }
        }
        else
        {
            Debug.LogWarning($"ID {id}나 {level} 레벨에 해당하는 데이터 없음");
        }
    }

    // 오버로딩을 통해 원하는 ID를 받아서 불러오는 용도
    // 시나리오, 투사체 등에서 사용할 예정
    private void ChangeProjectileID(int id)
    {
        if (_projectileData == null) return; // 투사체 데이터에 없으면 리턴

        // ProjectileDataList 에서 해당 레벨의 ID값의 캐릭터 찾기
        ProjectileDatas foundData = _projectileData.projectiles.Find(t => t.id == id);

        if (foundData != null)
        {
            ProjectileStats projectile = FindFirstObjectByType<ProjectileStats>(); // 현재 씬에서 배치된 오브젝트중 ProjectileStats 붙은 오브젝트 1개만 찾기
            if (projectile != null)
            {
                projectile.SetupValue(foundData); // ProjectileStats 에서 받은 능력치로 설정
            }
        }
        else
        {
            Debug.LogWarning($"ID {id}에 해당하는 데이터 없음");
        }
    }
    */
}