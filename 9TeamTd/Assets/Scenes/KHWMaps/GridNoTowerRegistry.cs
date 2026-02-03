using System.Collections.Generic;

/// <summary>
/// 타워 설치가 금지된 셀 목록 관리
/// GridSystem을 오염시키지 않기 위한 분리 레이어
/// </summary>
public static class GridNoTowerRegistry
{
    // 타워 설치 금지 셀을 저장하는 HashSet
    // HashSet을 사용하면 중복 없이 빠른 조회, 추가, 삭제 가능
    private static readonly HashSet<Cell> noTowerCells = new HashSet<Cell>();

    // 금지 셀 등록
    public static void Register(Cell cell)
    {
        // HashSet에 셀 추가
        noTowerCells.Add(cell);
        // 이미 존재하면 중복되지 않음 (HashSet 특성)
    }

    // 금지 셀 해제
    public static void Unregister(Cell cell)
    {
        // HashSet에서 셀 제거
        noTowerCells.Remove(cell);
        // 존재하지 않아도 오류 없이 처리됨
    }

    // 셀이 타워 설치 금지 영역인지 확인
    public static bool IsNoTowerCell(Cell cell)
    {
        // HashSet에 존재하면 true, 아니면 false
        return noTowerCells.Contains(cell);
    }
}
