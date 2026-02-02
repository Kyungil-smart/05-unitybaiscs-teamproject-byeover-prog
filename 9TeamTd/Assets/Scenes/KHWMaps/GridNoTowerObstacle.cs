using UnityEngine;

/// <summary>
/// 몬스터는 통과 가능
/// 타워만 설치 불가한 장애물
/// (강물, 연못 등)
/// </summary>
public class GridNoTowerObstacle : MonoBehaviour
{
    public Cell occupiedCell;

    public void Initialize(GridSystem gridSystem)
    {
        occupiedCell = gridSystem.WorldToCell(transform.position);
        GridNoTowerRegistry.Register(occupiedCell);
    }

    private void OnDestroy()
    {
        GridNoTowerRegistry.Unregister(occupiedCell);
    }
}