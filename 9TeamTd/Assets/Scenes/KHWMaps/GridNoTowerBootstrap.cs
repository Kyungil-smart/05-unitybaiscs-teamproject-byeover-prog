using UnityEngine;

/// <summary>
/// 씬 시작 시 NoTower 장애물 자동 등록
/// </summary>
public class GridNoTowerBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GridSystem grid = GridSystem.Instance;
        if (grid == null) return;

        GridNoTowerObstacle[] obstacles = FindObjectsOfType<GridNoTowerObstacle>();
        foreach (var obstacle in obstacles)
        {
            obstacle.Initialize(grid);
        }
    }
}
