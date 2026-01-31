using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 작성자: PEY

/// <summary>
/// 1x1 장애물에 부착
/// </summary>
public class GridObstacle : MonoBehaviour
{
    public Cell occupiedCell;

    public virtual void Initialize(GridSystem gridSystem)
    {
        occupiedCell = gridSystem.WorldToCell(transform.position);
    }
}