using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자: PEY

/// <summary>
/// 1x1 장애물에 부착 ,todo: 배열을 일반으로 바꿔야함
/// </summary>
public class GridObstacle : MonoBehaviour
{
    public Cell[] occupiedCells;

    public virtual void Initialize(GridSystem gridSystem)
    {
        occupiedCells = new Cell[1] { gridSystem.WorldToCell(transform.position) };
    }
}