using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnExampleScript : MonoBehaviour
{
    [Header("대상의 정보")]
    [SerializeField] private int targetID = 0;
    [SerializeField] private int targetLevel = -1;
    [SerializeField] private GameObject spawnObject;

    [Header("파일 유형 및 경로")]
    [SerializeField] private JsonType jsonType = JsonType.None;
    [SerializeField] string dataFilePath = "";  // Json 파일 경로

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

        // 게임 오브젝트 스폰
        Instantiate(spawnObject, spawnPosition, Quaternion.identity);
    }
}
