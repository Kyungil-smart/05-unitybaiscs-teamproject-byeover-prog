using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DrawTower : MonoBehaviour // 타워 배치 UI와 상호작용
{
    [SerializeField] GridSystem gridSystem;
    [SerializeField] GameObject[] TowerPrefabs;
    [HideInInspector] public static int targetIndex; // UI에서 선택한 타워 인덱스

    Vector3 worldPoint; Cell cell;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 플레이어의 위치에 worldPont를 설정
            worldPoint = transform.position;
            cell = gridSystem.WorldToCell(worldPoint);

            if (!gridSystem.IsCellOccupiedByMonster(cell) && gridSystem.TryPlaceTower(cell)) // 타워 배치 코드
            {
                TowerDatas data = JsonManager.instanceJsonManger.GetTowerData(UIController.toBuyTwID.Value, 1);

                if (Player.gold.Value < data.towerCost) return;
                Player.gold.Value -= data.towerCost;

                Instantiate(TowerPrefabs[targetIndex], gridSystem.CellToWorld(cell, y: 0f), Quaternion.identity);
            }
        }
    }

    public void OnBtn_SetTargetIndex(int index)
    {
        targetIndex = index;
    }
}

