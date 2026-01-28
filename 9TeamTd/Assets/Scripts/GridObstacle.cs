using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 그리드 상의 고정 장애물
/// 
/// 사용법:
/// 1. 장애물 오브젝트 생성 (바위, 나무, 건물 등)
/// 2. 이 컴포넌트 부착
/// 3. GridSystem이 자동으로 찾아서 경로 차단에 포함
/// </summary>
public class GridObstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("이 장애물이 차지하는 셀의 가로 크기")]
    [SerializeField, Min(1)] private int width = 1;
    [SerializeField] private int widthOffset = 0;

    [Tooltip("이 장애물이 차지하는 셀의 세로 크기")]
    [SerializeField, Min(1)] private int height = 1;
    [SerializeField] private int heightOffset = 0;

    [Header("Debug")]
    [SerializeField] private bool drawGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0.6f, 0.3f, 0.1f, 0.5f);

    /// <summary>
    /// 이 장애물이 차지하는 모든 셀 목록
    /// </summary>
    public Cell[] occupiedCells;

    public int Width => width;
    public int Height => height;

    /// <summary>
    /// 중심 셀 (왼쪽 아래 기준)
    /// </summary>
    public Cell CenterCell { get; private set; }

    /// <summary>
    /// GridSystem 참조를 받아 초기화
    /// </summary>
    public void Initialize(GridSystem gridSystem)
    {
        if (gridSystem == null)
        {
            Debug.LogError("[GridObstacle] GridSystem is null!");
            return;
        }

        // 현재 위치를 그리드 셀로 변환
        Vector3 offsetedPosition = transform.position + new Vector3(widthOffset * gridSystem.CellSize, 0, heightOffset * gridSystem.CellSize);
        CenterCell = gridSystem.WorldToCell(offsetedPosition);

        // 차지하는 모든 셀 계산
        CalculateOccupiedCells(gridSystem);
    }

    /// <summary>
    /// 이 장애물이 차지하는 모든 셀 계산
    /// </summary>
    private void CalculateOccupiedCells(GridSystem gridSystem)
    {
        occupiedCells = new Cell[width * height];
        int index = 0;

        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                Cell cell = new Cell(CenterCell.X + dx, CenterCell.Y + dy);
                occupiedCells[index++] = cell;
            }
        }
    }

    /// <summary>
    /// Gizmo로 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!drawGizmo)
            return;

        // GridSystem 찾기 (에디터용)
        GridSystem gridSystem = FindObjectOfType<GridSystem>();
        if (gridSystem == null)
            return;
        
        Vector3 offsetedPosition = transform.position + new Vector3(widthOffset * gridSystem.CellSize, 0, heightOffset * gridSystem.CellSize);
        Cell centerCell = gridSystem.WorldToCell(offsetedPosition);
        Gizmos.color = gizmoColor;

        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                Cell cell = new Cell(centerCell.X + dx, centerCell.Y + dy);
                Vector3 worldPos = gridSystem.CellToWorld(cell, 0.1f);
                Vector3 size = new Vector3(
                    gridSystem.CellSize * 0.9f,
                    0.2f,
                    gridSystem.CellSize * 0.9f
                );
                Gizmos.DrawCube(worldPos, size);
            }
        }
    }
}