using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자 : 한성우

public class SpawnExampleScript : MonoBehaviour
{
    [Header("대상의 정보")]
    [SerializeField] private int targetID = 0;
    [SerializeField] private int targetLevel = -1;
    [SerializeField] private GameObject spawnObject;

    [Header("파일 유형 및 경로")]
    [SerializeField] private JsonType jsonType = JsonType.None;
    [SerializeField] private string dataFilePath = "";  // Json 파일 경로

    [Header("스폰 위치")]
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;

    private Coroutine DelaySpawn;

    private void Start()
    {
        // 잠시 대기 (시작하자마자 스폰되면 스폰 안보여서 추가)
        DelaySpawn = StartCoroutine(DelaySpawnCor(3f));
    }

    IEnumerator DelaySpawnCor(float waitSec)
    {
        yield return new WaitForSeconds(waitSec);
        Debug.Log("3초 대기 완료");

        SpawnObject();
    }

    private void SpawnObject()
    {
        if (JsonManager.instanceJsonManger == null)
        {
            Debug.LogError("JsonManager 없음");
            return;
        }

        // 게임 오브젝트 스폰 및 데이터 가져오기 (타워의 경우)
        GameObject spawnTower = Instantiate(spawnObject, spawnPosition, Quaternion.identity);
        TowerStats towerStats = spawnTower.GetComponent<TowerStats>();

        if (towerStats != null)
        {
           
            // Json 매니저에게 요청
            TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(targetID, targetLevel);
            
            if (data != null)
            {
                // 가져온 데이터 주입
                towerStats.SetupValue(data);
            }
        }
        else
        {
            Debug.LogError("스폰된 오브젝트 'TowerStats' 없음");
        }
    }
}
