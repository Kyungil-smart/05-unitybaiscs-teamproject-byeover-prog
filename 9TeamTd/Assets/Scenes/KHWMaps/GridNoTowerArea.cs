using UnityEngine;

// BoxCollider가 반드시 붙어있도록 강제
[RequireComponent(typeof(BoxCollider))]
public class GridNoTowerArea : MonoBehaviour
{
    // Start는 게임 시작 시 한 번 실행됨
    private void Start()
    {
        // 싱글톤으로 구현된 GridSystem 인스턴스 가져오기
        GridSystem grid = GridSystem.Instance;

        // GridSystem이 없으면 에러 로그 출력 후 종료
        if (grid == null)
        {
            Debug.LogError("[GridNoTowerArea] GridSystem.Instance is null");
            return;
        }

        // 현재 오브젝트의 BoxCollider 가져오기
        BoxCollider box = GetComponent<BoxCollider>();

        // BoxCollider의 월드 좌표 최소값
        Vector3 min = box.bounds.min;

        // BoxCollider의 월드 좌표 최대값
        Vector3 max = box.bounds.max;

        // 최소 월드 좌표를 그리드 셀 좌표로 변환
        Cell minCell = grid.WorldToCell(min);

        // 최대 월드 좌표를 그리드 셀 좌표로 변환
        Cell maxCell = grid.WorldToCell(max);

        // 최소 셀부터 최대 셀까지 반복 (y 축 먼저)
        for (int y = minCell.Y; y <= maxCell.Y; y++)
        {
            for (int x = minCell.X; x <= maxCell.X; x++)
            {
                // 현재 셀 생성
                Cell cell = new Cell(x, y);

                // 셀이 그리드 범위 안에 있는지 확인
                if (!grid.IsInside(cell))
                    continue; // 범위 밖이면 건너뜀

                // No-Tower 영역으로 등록
                GridNoTowerRegistry.Register(cell);

                // 에디터 모드에서 디버그용 출력
#if UNITY_EDITOR
                Debug.Log($"[NoTower] Register {cell}");
#endif
            }
        }
    }

    // 오브젝트가 삭제될 때 실행
    private void OnDestroy()
    {
        // GridSystem 인스턴스 가져오기
        GridSystem grid = GridSystem.Instance;
        if (grid == null) return;

        // BoxCollider 가져오기
        BoxCollider box = GetComponent<BoxCollider>();

        // 최소, 최대 월드 좌표 계산
        Vector3 min = box.bounds.min;
        Vector3 max = box.bounds.max;

        // 월드 좌표 → 셀 좌표 변환
        Cell minCell = grid.WorldToCell(min);
        Cell maxCell = grid.WorldToCell(max);

        // 최소~최대 셀 범위 반복
        for (int y = minCell.Y; y <= maxCell.Y; y++)
        {
            for (int x = minCell.X; x <= maxCell.X; x++)
            {
                // No-Tower 영역에서 제거
                GridNoTowerRegistry.Unregister(new Cell(x, y));
            }
        }
    }
}
