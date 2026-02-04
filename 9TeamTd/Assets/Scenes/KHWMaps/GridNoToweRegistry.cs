using System.Collections.Generic;

/// <summary>
/// 타워 설치가 금지된 셀 목록 관리
/// GridSystem을 오염시키지 않기 위한 분리 레이어
/// </summary>
public static class GridNoTowerRegistry
{
    private static readonly HashSet<Cell> noTowerCells = new HashSet<Cell>();

    public static void Register(Cell cell)
    {
        noTowerCells.Add(cell);
    }

    public static void Unregister(Cell cell)
    {
        noTowerCells.Remove(cell);
    }

    public static bool IsNoTowerCell(Cell cell)
    {
        return noTowerCells.Contains(cell);
    }
}
