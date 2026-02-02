using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DrawTower : MonoBehaviour // 타워 배치 UI와 상호작용
{
    [SerializeField] GameObject[] TowerPrefabs;
    [HideInInspector] public static int targetIndex; // UI에서 선택한 타워 인덱스

    Vector3 worldPoint; Cell cell;

    private void Start()
    {
        worldPoint = transform.position;
        cell = GridSystem.Instance.WorldToCell(worldPoint);
        GridSystem.Instance.TryPlaceTower(cell);
        GridSystem.Instance.SetCellState(cell, GridSystem.CellState.Empty);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(UIController.toBuyTwID.Value, 1);
            if (StageManager.gold.Value < data.towerCost) return;

            // 플레이어의 위치에 worldPont를 설정
            worldPoint = transform.position;
            cell = GridSystem.Instance.WorldToCell(worldPoint);

            if (!GridSystem.Instance.IsCellOccupiedByMonster(cell) && GridSystem.Instance.TryPlaceTower(cell)) // 타워 배치 코드
            {
                StageManager.gold.Value -= data.towerCost;

                Instantiate(TowerPrefabs[targetIndex], GridSystem.Instance.CellToWorld(cell, y: 0f), Quaternion.identity);
            }
        }
    }

    public void OnBtn_SetTargetIndex(int index)
    {
        targetIndex = index;
    }
}

