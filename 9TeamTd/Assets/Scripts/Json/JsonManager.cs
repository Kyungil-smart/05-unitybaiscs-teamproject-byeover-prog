using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static TowerStats;
using static UnityEngine.GraphicsBuffer;

// 작성자 : 한성우

// https://3dperson1.tistory.com/117 스크립트 기반 -> 추후 수정 예정
public class JsonManager : MonoBehaviour
{
    public static JsonManager instanceJsonManger { get; private set; }

    [Header("대상의 정보")]
    [SerializeField] private int targetID = 0;
    [SerializeField] private int targetLevel = 0;

    [Header("Json 파일 경로")]
    [SerializeField] private string dataFilePath = "";  // Json 파일 경로


    private TowerDataList _towerData;   // TowerStats 의 데이터를 불러오면 됨

    private void Awake()
    {
        // 이 매니저가 2개 이상 있으면 1개는 삭제
        #region 싱글톤
        if (instanceJsonManger == null)
        {
            instanceJsonManger = this;
            DontDestroyOnLoad(gameObject);
            LoadJsonFile(); // 파일 불러오기
        }
        else
        {
            Destroy(gameObject);
        }
        #endregion
    }

    public void Start()
    {
        ChangeID(targetID, targetLevel);
    }

    private void LoadJsonFile()
    {
        TextAsset jsonDataFile = Resources.Load<TextAsset>(dataFilePath);    // TextAsset(텍스트 파일 형식) 으로 리소스 폴더 하위 경로에서 TowerData 파일을 불러옴//TextAsset jsonDataFile = Resources.Load<TextAsset>("Datas/TowerData");    // TextAsset(텍스트 파일 형식) 으로 리소스 폴더 하위 경로에서 TowerData 파일을 불러옴
        if (jsonDataFile != null)
        {
            _towerData = JsonUtility.FromJson<TowerDataList>(jsonDataFile.text);    // JsonUtility 활용해 TowerDataList 역직렬화
        }
        else
        {
            Debug.LogError("파일 없음");
        }
    }

    /* 아래 ChangeID 기능은 현재 TowerDatas 만 읽을 수 있어서 수정 필요 */

    // 원하는 ID와 레벨을 받아서 불러오는 용도
    // 타워, 몬스터, 기지 등에서 사용할 예정
    public void ChangeID(int id, int level)
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
    public void ChangeID(int id)
    {
        if (_towerData == null) return; // 타워 데이터에 없으면 리턴

        // TowerDataList에서 해당 레벨의 ID값의 캐릭터 찾기
        TowerDatas foundData = _towerData.towers.Find(t => t.id == id);

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
            Debug.LogWarning($"ID {id}에 해당하는 데이터 없음");
        }
    }
}
