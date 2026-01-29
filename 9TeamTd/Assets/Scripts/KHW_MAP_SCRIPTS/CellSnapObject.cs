using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배치된 오브젝트를 자동으로 셀 중앙에 스냅시키고
/// 해당 셀을 Blocked로 등록
/// </summary>
[ExecuteAlways]
public sealed class CellSnapObject : MonoBehaviour
{
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private bool registerAsObstacle = true;

    private Cell snappedCell;

    private void OnEnable()
    {
        if (gridSystem == null)
            gridSystem = FindObjectOfType<GridSystem>();

        Snap();
    }

    private void OnValidate()
    {
        Snap();
    }

    private void Snap()
    {
        if (gridSystem == null)
            return;

        Cell cell = gridSystem.WorldToCell(transform.position);
        Vector3 center = gridSystem.CellToWorld(cell, transform.position.y);
        transform.position = center;

        snappedCell = cell;

        if (registerAsObstacle)
            gridSystem.TryPlaceTower(cell); // 길막 셀로 등록
    }
}
