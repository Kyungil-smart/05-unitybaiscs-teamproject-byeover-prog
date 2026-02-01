using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public partial class GridSystem : MonoBehaviour
{
    // 싱글톤 추가
    public static GridSystem Instance { get; private set; }
    
    private const string BaseTag = "Base";

    [Header("Grid")]
    [FormerlySerializedAs("grid_width")]
    [SerializeField, Min(2)] private int gridWidth = 30;

    [FormerlySerializedAs("grid_height")]
    [SerializeField, Min(2)] private int gridHeight = 20;

    [FormerlySerializedAs("cell_size")]
    [SerializeField, Min(0.1f)] private float cellSize = 1f;

    [Header("Base")]
    [FormerlySerializedAs("base_transform")]
    [SerializeField] private Transform baseTransform;

    // baseTransform이 비어있을 때 사용할 대체 기지 셀
    [FormerlySerializedAs("base_cell")]
    [SerializeField] private Cell fallbackBaseCell = new Cell(15, 10);

    [Header("Towers")]
    [FormerlySerializedAs("tower_prefab")]
    [Tooltip("타워 프리팹이 없으면 기본 Cube로 생성.")]
    [SerializeField] private GameObject towerPrefab;

    [FormerlySerializedAs("tower_height")]
    [SerializeField, Min(0.1f)] private float towerHeight = 1f;

    [Header("Placement Rules")]
    [FormerlySerializedAs("no_build_border_thickness")]
    [Tooltip("맵 가장자리에서 2만큼은 설치 금지 구역으로 만든다.")] // 2칸이 몬스터 나오는 곳이라서 타워 설치하면 오류남
    [SerializeField, Min(0)] private int noBuildBorderThickness = 2;

    [FormerlySerializedAs("prevent_build_on_monster")]
    [SerializeField] private bool preventBuildOnMonster = true;

    [FormerlySerializedAs("monster_layer_mask")]
    [SerializeField] private LayerMask monsterLayerMask;

    [FormerlySerializedAs("monster_check_y")]
    [SerializeField, Min(0f)] private float monsterCheckY = 0.5f;

    [FormerlySerializedAs("monster_check_half_height")]
    [SerializeField, Min(0.01f)] private float monsterCheckHalfHeight = 1.0f;

    [Header("Debug")]
    [FormerlySerializedAs("draw_grid_gizmos")]
    [SerializeField] private bool drawGridGizmos = true;

    [FormerlySerializedAs("draw_blocked_cells")]
    [SerializeField] private bool drawBlockedCells = true;

    [FormerlySerializedAs("draw_unreachable_cells")]
    [SerializeField] private bool drawUnreachableCells = false;

    public int Width => gridWidth;
    public int Height => gridHeight;
    public float CellSize => cellSize;
    public Cell BaseCell => baseCell;

    public enum CellState : byte
    {
        Empty = 0,
        Blocked = 1,
        Base = 2,
    }

    private static readonly Vector2Int[] CardinalDirections =
    {
        new Vector2Int(0, 1),   // Up
        new Vector2Int(1, 0),   // Right
        new Vector2Int(0, -1),  // Down
        new Vector2Int(-1, 0),  // Left
    };

    [HideInInspector] public CellState[] cellStates;
    private int[] distanceToBase;         // (몬스터가 사용하는 실제 필드)
                                          // Real distance field used by monsters

    private int[] previewDistanceToBase;  // (배치 미리보기에 사용되는 거리 필드)
                                          // Scratch distance field used by placement preview

    private Cell baseCell;

    private readonly Dictionary<int, GameObject> towerVisualByIndex = new Dictionary<int, GameObject>();
    private readonly List<Cell> edgeSpawnBuffer = new List<Cell>(128);
    private readonly Collider[] monsterOverlapBuffer = new Collider[8];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        ResolveReferencesIfNeeded();
        ClampSettings();
        EnsureBuffers();

        ResolveBaseCell();
        ResetGridState();
        RebuildDistanceField(distanceToBase, assumedBlockedCell: null);

        RegisterObstacles(); // ��ֹ� ��� �߰���

        SnapBaseTransformToCellCenter();
    }

    /// <summary>
    /// 인스펙터에서 스크립트의 속성이 수정될 때마다 호출되는 메서드
    /// </summary>
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        ClampSettings();
        EnsureBuffers();

        ResolveBaseCellFromCurrentReferences();
        ResetGridState();
        RebuildDistanceField(distanceToBase, assumedBlockedCell: null);
    }

    // -----------------------
    // Coordinate conversion
    // -----------------------
    /// <summary>
    /// 월드 공간의 3D 위치를 그리드 상의 2D 셀 좌표로 변환하는 메서드
    /// </summary>
    /// <remarks>
    /// 월드 좌표의 X축은 그리드의 X(열), Z축은 그리드의 Y(행)
    /// 소수점 좌표는 FloorToInt로 인해 내림으로 처리되어 칸의 인덱스를 반환
    /// </remarks>
    /// <param name="worldPosition">변환할 월드 좌표(Vector3)</param>
    /// <returns>해당 위치가 포함된 그리드 셀(Cell)</returns>
    public Cell WorldToCell(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize);
        return new Cell(x, y);
    }

    /// <summary>
    /// 그리드 좌표를 실제 유니티 좌표로 변환해주는 계산기 메서드
    /// </summary>
    /// <param name="cell">변환할 그리드 셀 좌표</param>
    /// <param name="y">반환할 월드 좌표의 높이 값</param>
    /// <returns></returns>
    public Vector3 CellToWorld(Cell cell, float y = 0f)
    {
        // 좌표 계산시 0.5를 더해 그리드 칸의 정중앙을 구함
        float worldX = (cell.X + 0.5f) * cellSize;
        float worldZ = (cell.Y + 0.5f) * cellSize;
        return new Vector3(worldX, y, worldZ);
    }

    /// <summary>
    /// 입력된 셀 좌표가 그리드 경계 내부에 있는지 검사하는 메서드
    /// </summary>
    /// <param name="cell">검사할 때 사용하는 셀 좌표</param>
    /// <returns>좌표가 그리드 안에 있으면 true, 벗어나 있으면 false 반환</returns>
    public bool IsInside(Cell cell)
    {
        return cell.X >= 0 && cell.X < gridWidth && cell.Y >= 0 && cell.Y < gridHeight;
    }

    /// <summary>
    /// 타워 설치가 가능한지 검사하는 메서드
    /// </summary>
    /// <param name="cell">건설을 시도할 때 사용할 셀 좌표</param>
    /// <returns>건설 가능하면 true, 불가능하면 false</returns>
    public bool IsBuildable(Cell cell)
    {
        // 1) Basic bounds / static rules first
        if (!IsInside(cell)) return false;
        if (cell == baseCell) return false;
        if (GetCellState(cell) != CellState.Empty) return false;

        // 가장자리 (스폰구역) 건설 금지
        if (cell.X < noBuildBorderThickness || cell.X >= gridWidth - noBuildBorderThickness ||
            cell.Y < noBuildBorderThickness || cell.Y >= gridWidth - noBuildBorderThickness)
            return false;
        
        return true;
        // 2) Dynamic rule: 몬스터 위/근처 설치 금지(필요할 때 주석 해제)
        //if (IsCellOccupiedByMonster(cell))
        //    return false;
    }

    /// <summary>
    /// 타워 건설 시, 게임 진행에 문제가 없는지 검사하는 메서드
    /// </summary>
    /// <remarks>
    /// assumedBlockedCell이라는 가상벽을 세워
    /// 몬스터가 기지까지 도달할 수 있는지 시뮬레이션 돌리는 메서드
    /// </remarks>
    /// <param name="cell">검사할 셀 좌표</param>
    /// <returns>건설 가능하고 길도 막히지 않았으면 true</returns>
    public bool CanPlaceTower(Cell cell)
    {
        if (!IsBuildable(cell))
            return false;

        RebuildDistanceField(previewDistanceToBase, assumedBlockedCell: cell);
        return HasAnyReachableEdgeSpawn(previewDistanceToBase, assumedBlockedCell: cell);
    }

    /// <summary>
    /// 실제로 타워를 설치할 수 있는지 시도하는 메서드
    /// </summary>
    /// <remarks>
    /// 먼저, 셀을 Blocked로 설정해놓고,
    /// 몬스터가 Base로 지나갈 셀이 끊겼다면 Cell을
    /// Bloked에서 Empty로 롤백
    /// </remarks>
    /// <param name="cell">타워 건설을 시도할 셀 좌표</param>
    /// <returns>건설에 성공했다면 true, 길을 막거나 건설이 불가능하면 false</returns>
    public bool TryPlaceTower(Cell cell)
    {
        if (!IsBuildable(cell))
        {
#if UNITY_EDITOR
            Debug.Log($"[GridSystem] TryPlaceTower failed (NotBuildable) cell={cell}");
#endif
            return false;
        }

        SetCellState(cell, CellState.Blocked);
        RebuildDistanceField(distanceToBase, assumedBlockedCell: null);

        if (!HasAnyReachableEdgeSpawn(distanceToBase, assumedBlockedCell: null))
        {
            // Rollback
            SetCellState(cell, CellState.Empty);
            RebuildDistanceField(distanceToBase, assumedBlockedCell: null);
#if UNITY_EDITOR
            Debug.Log($"[GridSystem] TryPlaceTower failed (WouldBlockAllSpawns) cell={cell}");
#endif
            return false;
        }

        //SpawnTowerVisual(cell);
#if UNITY_EDITOR
        Debug.Log($"[GridSystem] TryPlaceTower success cell={cell}");
#endif
        return true;
    }

    /// <summary>
    /// 설치된 타워를 제거하고 베이스까지의 경로를 재계산하는 메서드
    /// </summary>
    /// <param name="cell">철거할 타워가 있는 셀 좌표</param>
    /// <returns>철거에 성공했으면 true, 타워가 없거나 맵 밖이면 false</returns>
    public bool RemoveTower(Cell cell)
    {
        // 유효성 검사
        if (!IsInside(cell))
            return false;

        if (GetCellState(cell) != CellState.Blocked)
            return false;

        int index = ToIndex(cell);

        if (towerVisualByIndex.TryGetValue(index, out GameObject visual) && visual != null)
        {
            Destroy(visual);
            towerVisualByIndex.Remove(index);
        }

        SetCellState(cell, CellState.Empty);
        RebuildDistanceField(distanceToBase, assumedBlockedCell: null);

#if UNITY_EDITOR
        Debug.Log($"[GridSystem] RemoveTower success cell={cell}");
#endif
        return true;
    }

    // -----------------------
    // Monster occupancy (dynamic rule)
    // -----------------------
    /// <summary>
    /// 해당 셀 위치에 몬스터가 존재하는지 검사하는 메서드 (Physic 사용)
    /// </summary>
    /// <remarks>
    /// 타워를 건설할 때 몬스터 바로 위에 짓는 것을 막기 위해 사용
    /// </remarks>
    /// <param name="cell">검사할 셀 좌표</param>
    /// <returns>몬스터가 있으면 true, 없으면 false</returns>
    public bool IsCellOccupiedByMonster(Cell cell)
    {
        if (!preventBuildOnMonster)
            return false;

        // 물리 연산은 게임 실행중에만
        if (!Application.isPlaying)
            return false;

        if (!IsInside(cell))
            return false;

        Vector3 center = CellToWorld(cell, y: monsterCheckY);

        // 중요!! : 수정할 때 cellSize 변수 사용 (cell_size 절대 금지)
        Vector3 halfExtents = new Vector3(cellSize * 0.45f, monsterCheckHalfHeight, cellSize * 0.45f);


        int hitCount = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            monsterOverlapBuffer,
            Quaternion.identity,
            monsterLayerMask,
            QueryTriggerInteraction.Ignore
        );

        return hitCount > 0;
    }

    // -----------------------
    // Spawn / path following
    // -----------------------
    /// <summary>
    /// 목표 지점까지 도달 가능한 가장자리 셀 중 하나를 무작위로 반환하는 메서드
    /// </summary>
    /// <param name="spawnCell">선택된 스폰 위치의 그리드 좌표</param>
    /// <returns>경로가 막혀있거나 스폰 가능한 곳이 없으면 false, 스폰 가능한 위치면 true</returns>
    public bool TryGetRandomSpawnCell(out Cell spawnCell)
    {
        CollectReachableEdgeCells(edgeSpawnBuffer);

        if (edgeSpawnBuffer.Count == 0)
        {
            spawnCell = default;
            Debug.LogWarning("[GridSystem] No reachable edge spawn cells!");
            return false;
        }

        spawnCell = edgeSpawnBuffer[Random.Range(0, edgeSpawnBuffer.Count)];
        return true;
    }

    /// <summary>
    /// 현재 위치에서 베이스로 향하는 다음 이동 경로를 계산하는 메서드
    /// </summary>
    /// <param name="currentCell">몬스터의 현재 그리드 좌표</param>
    /// <param name="lastDir">몬스터가 직전의 이동해온 방향</param>
    /// <param name="nextCell">이동할 다음 그리드 좌표</param>
    /// <param name="nextDir">이동할 다음 방향 벡터</param>
    /// <returns>이동할 다음 경로를 찾았다면 true, 이미 도착했거나 경로가 없으면 false</returns>
    public bool TryGetNextStep(Cell currentCell, Vector2Int lastDir, out Cell nextCell, out Vector2Int nextDir)
    {
        nextCell = currentCell;
        nextDir = Vector2Int.zero;

        if (!IsInside(currentCell))
            return false;

        int currentDistance = GetDistance(distanceToBase, currentCell);
        if (currentDistance <= 0)
            return false; // 0 = base, -1 = unreachable

        // Straight-first
        if (lastDir != Vector2Int.zero)
        {
            Cell straightCell = new Cell(currentCell.X + lastDir.x, currentCell.Y + lastDir.y);
            if (IsInside(straightCell) && GetDistance(distanceToBase, straightCell) == currentDistance - 1)
            {
                nextCell = straightCell;
                nextDir = lastDir;
                return true;
            }
        }

        // Fallback: fixed priority
        for (int i = 0; i < CardinalDirections.Length; i++)
        {
            Vector2Int dir = CardinalDirections[i];
            Cell candidate = new Cell(currentCell.X + dir.x, currentCell.Y + dir.y);

            if (!IsInside(candidate))
                continue;

            if (GetDistance(distanceToBase, candidate) == currentDistance - 1)
            {
                nextCell = candidate;
                nextDir = dir;
                return true;
            }
        }

        return false;
    }

    // -----------------------
    // Internal setup
    // -----------------------
    /// <summary>
    /// 베이스(목표 지점) 트랜스폼 참조가 누락됐을 때, 태그를 통해 자동으로 참조하는 메서드
    /// </summary>
    private void ResolveReferencesIfNeeded()
    {
        if (baseTransform == null)
            return;

        try
        {
            GameObject baseObj = GameObject.FindWithTag(BaseTag);
            if (baseObj != null)
                baseTransform = baseObj.transform;
        }
        catch (UnityException)
        {
            // Tag missing is fine; fallbackBaseCell will be used.
        }
    }

    private void ClampSettings()
    {
        if (gridWidth < 2) gridWidth = 2;
        if (gridHeight < 2) gridHeight = 2;
        if (cellSize < 0.1f) cellSize = 0.1f;
        if (noBuildBorderThickness < 0) noBuildBorderThickness = 0;
        if (towerHeight < 0.1f) towerHeight = 0.1f;
    }

    private void EnsureBuffers()
    {
        int expectedLength = gridWidth * gridHeight;

        if (cellStates == null || cellStates.Length != expectedLength)
            cellStates = new CellState[expectedLength];

        if (distanceToBase == null || distanceToBase.Length != expectedLength)
            distanceToBase = new int[expectedLength];

        if (previewDistanceToBase == null || previewDistanceToBase.Length != expectedLength)
            previewDistanceToBase = new int[expectedLength];
    }

    private void ResolveBaseCell()
    {
        ResolveBaseCellFromCurrentReferences();

        if (!IsInside(baseCell))
            baseCell = new Cell(gridWidth / 2, gridHeight / 2);
    }

    private void ResolveBaseCellFromCurrentReferences()
    {
        if (baseTransform != null)
        {
            baseCell = WorldToCell(baseTransform.position);
            return;
        }

        baseCell = fallbackBaseCell;
    }

    private void ResetGridState()
    {
        for (int i = 0; i < cellStates.Length; i++)
            cellStates[i] = CellState.Empty;

        if (IsInside(baseCell))
            cellStates[ToIndex(baseCell)] = CellState.Base;
    }

    private void SnapBaseTransformToCellCenter()
    {
        if (baseTransform == null)
            return;

        Vector3 aligned = CellToWorld(baseCell, y: baseTransform.position.y);
        baseTransform.position = aligned;
    }

    // -----------------------
    // Grid storage helpers
    // -----------------------
    public int ToIndex(Cell cell) => cell.Y * gridWidth + cell.X;

    public CellState GetCellState(Cell cell) => cellStates[ToIndex(cell)];

    public void SetCellState(Cell cell, CellState state) => cellStates[ToIndex(cell)] = state;

    public bool IsCellWalkable(Cell cell, Cell? assumedBlockedCell)
    {
        if (assumedBlockedCell.HasValue && cell == assumedBlockedCell.Value)
            return false;

        CellState state = GetCellState(cell);
        return state == CellState.Empty || state == CellState.Base;
    }

    // -----------------------
    // BFS distance field (Pathfinding)
    // -----------------------

    /// <summary>
    /// 특정한 셀에서 베이스까지의 최단 거리를 반환하는 메서드
    /// </summary>
    /// <param name="distanceField">참조할 거리 필드 배열</param>
    /// <param name="cell">거리를 알고 싶은 셀 위치</param>
    /// <returns>목표까지의 거리, 도달 불가능하면 -1</returns>
    public int GetDistance(int[] distanceField, Cell cell) => distanceField[ToIndex(cell)];

    /// <summary>
    /// 중요 BFS 알고리즘 사용해서 베이스에서 모든 셀까지의 거리를 계산하는 메서드
    /// </summary>
    /// <param name="outDistanceField">계산될 거리를 저장할 배열</param>
    /// <param name="assumedBlockedCell">경로 계산 시 장애물로 간주할 셀</param>
    public void RebuildDistanceField(int[] outDistanceField, Cell? assumedBlockedCell)
    {
        for (int i = 0; i < outDistanceField.Length; i++)
            outDistanceField[i] = -1;

        if (!IsCellWalkable(baseCell, assumedBlockedCell))
        {
            Debug.LogError("[GridSystem] Base cell is not walkable. Check grid state.");
            return;
        }

        Queue<Cell> queue = new Queue<Cell>(256);

        outDistanceField[ToIndex(baseCell)] = 0;
        queue.Enqueue(baseCell);

        while (queue.Count > 0)
        {
            Cell current = queue.Dequeue();
            int currentDistance = outDistanceField[ToIndex(current)];

            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                Vector2Int dir = CardinalDirections[i];
                Cell neighbor = new Cell(current.X + dir.x, current.Y + dir.y);

                if (!IsInside(neighbor))
                    continue;

                if (!IsCellWalkable(neighbor, assumedBlockedCell))
                    continue;

                int neighborIndex = ToIndex(neighbor);
                if (outDistanceField[neighborIndex] != -1)
                    continue;

                outDistanceField[neighborIndex] = currentDistance + 1;
                queue.Enqueue(neighbor);
            }
        }
    }

    // -----------------------
    // Edge spawn validation
    // -----------------------
    /// <summary>
    /// 맵 가장자리 중, 베이스까지 도달 가능한 스폰 지점이 하나라도 있는지 검사해주는 메서드
    /// </summary>
    /// <param name="distanceField">검사할 거리 필드</param>
    /// <param name="assumedBlockedCell">검사할 때 이용할 가상 벽</param>
    /// <returns>몬스터가 스폰될 지점이 하나라도 있으면 true, 아예 없으면 false</returns>
    private bool HasAnyReachableEdgeSpawn(int[] distanceField, Cell? assumedBlockedCell)
    {
        // 상단 (y=0)
        for (int x = 0; x < gridWidth; x++)
        {
            if (IsReachableSpawnCell(new Cell(x, 0), distanceField, assumedBlockedCell))
                return true;
        }

        // 하단(y=gridHeight-1)
        int bottomY = gridHeight - 1;
        if (bottomY != 0)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (IsReachableSpawnCell(new Cell(x, bottomY), distanceField, assumedBlockedCell))
                    return true;
            }
        }

        // 좌/우 엣지(y=1~gridHeight-2)
        for (int y = 1; y < gridHeight - 1; y++)
        {
            if (IsReachableSpawnCell(new Cell(0, y), distanceField, assumedBlockedCell))
                return true;

            int rightX = gridWidth - 1;
            if (rightX != 0 && IsReachableSpawnCell(new Cell(rightX, y), distanceField, assumedBlockedCell))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 특정 셀이 유효한 몬스터 스폰 위치인지 검증하는 메서드
    /// </summary>
    /// <remarks>
    /// 1. 그리드 범위 내부인가?
    /// 2. 베이스가 아닌가?
    /// 3. 이동 가능한 타일인가? (타워나 장애물이 없어야함)
    /// 4. 베이스까지의 경로가 존재하는가?
    /// 이 4가지 조건을 모두 만족해야지 true를 반환
    /// </remarks>
    /// <param name="cell">검사할 셀 좌표</param>
    /// <param name="distanceField">참조할 거리 필드 배열</param>
    /// <param name="assumedBlockedCell">검사할 때 이용할 가상 벽</param>
    /// <returns>스폰 가능한 위치면 true, 불가능하면 false</returns>
    private bool IsReachableSpawnCell(Cell cell, int[] distanceField, Cell? assumedBlockedCell)
    {
        if (!IsInside(cell)) return false;
        if (cell == baseCell) return false;

        if (!IsCellWalkable(cell, assumedBlockedCell))
            return false;

        return GetDistance(distanceField, cell) != -1;
    }

    /// <summary>
    /// 맵의 가장자리에 있는 셀들 중, 스폰 가능한 곳을 모두 찾아서 리스트에 저장하는 메서드
    /// </summary>
    /// <param name="buffer">찾아낸 스폰 지점들을 저장할 리스트</param>
    private void CollectReachableEdgeCells(List<Cell> buffer)
    {
        buffer.Clear();

        // Top row
        for (int x = 0; x < gridWidth; x++)
            AddIfReachableSpawnCell(new Cell(x, 0), buffer);

        // Bottom row
        int bottomY = gridHeight - 1;
        if (bottomY != 0)
        {
            for (int x = 0; x < gridWidth; x++)
                AddIfReachableSpawnCell(new Cell(x, bottomY), buffer);
        }

        // Left/right columns without corners
        int rightX = gridWidth - 1;
        for (int y = 1; y < gridHeight - 1; y++)
        {
            AddIfReachableSpawnCell(new Cell(0, y), buffer);

            if (rightX != 0)
                AddIfReachableSpawnCell(new Cell(rightX, y), buffer);
        }
    }

    /// <summary>
    /// 해당 셀이 스폰 조건을 만족하면 버퍼에 추가하는 메서드
    /// </summary>
    /// <param name="cell">검사할 셀 좌표</param>
    /// <param name="buffer">조건 만족 시 추가할 리스트</param>
    private void AddIfReachableSpawnCell(Cell cell, List<Cell> buffer)
    {
        if (!IsReachableSpawnCell(cell, distanceToBase, assumedBlockedCell: null))
            return;

        if (GetCellState(cell) != CellState.Empty)
            return;

        buffer.Add(cell);
    }

    // -----------------------
    // Tower visuals
    // -----------------------
    public void SpawnTowerVisual(Cell cell)
    {
        int index = ToIndex(cell);

        if (towerVisualByIndex.TryGetValue(index, out GameObject existing) && existing != null)
            return;

        Vector3 position = CellToWorld(cell, y: towerHeight * 0.5f);

        GameObject towerObj;
        if (towerPrefab != null)
        {
            towerObj = Instantiate(towerPrefab, position, Quaternion.identity);
        }
        else
        {
            towerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            towerObj.transform.position = position;

            float side = cellSize * 0.9f;
            towerObj.transform.localScale = new Vector3(side, towerHeight, side);
        }

        towerObj.name = $"Tower_{cell.X}_{cell.Y}";
        towerVisualByIndex[index] = towerObj;
    }

    /// <summary>
    /// 장애물 오브젝트들을 그리드에 등록하는 메서드 (수정됨)
    /// </summary>
    private void RegisterObstacles()
    {
        GridObstacle[] obstacles = FindObjectsOfType<GridObstacle>();
        GridObstacleBIG[] bigObstacles = FindObjectsOfType<GridObstacleBIG>();

        foreach (GridObstacle obstacle in obstacles)
        {
            obstacle.Initialize(this);

            Cell cell = obstacle.occupiedCell;
            {
                SetCellState(cell, CellState.Blocked);
            }
        }
        foreach (GridObstacleBIG bigObstacle in bigObstacles)
        {
            bigObstacle.Initialize(this);

            foreach (Cell cell in bigObstacle.occupiedCells)
            {
                SetCellState(cell, CellState.Blocked);
            }
        }
    }

    // -----------------------
    // Gizmos
    // -----------------------
    public void OnDrawGizmos()
    {
        if (!drawGridGizmos)
            return;

        if (cellStates == null || distanceToBase == null)
            return;

        int expectedLength = gridWidth * gridHeight;
        if (cellStates.Length != expectedLength || distanceToBase.Length != expectedLength)
            return;

        Gizmos.color = new Color(1f, 1f, 1f, 0.15f);

        float worldW = gridWidth * cellSize;
        float worldH = gridHeight * cellSize;

        for (int x = 0; x <= gridWidth; x++)
        {
            float px = x * cellSize;
            Gizmos.DrawLine(new Vector3(px, 0f, 0f), new Vector3(px, 0f, worldH));
        }

        for (int y = 0; y <= gridHeight; y++)
        {
            float pz = y * cellSize;
            Gizmos.DrawLine(new Vector3(0f, 0f, pz), new Vector3(worldW, 0f, pz));
        }

        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.6f);
        Gizmos.DrawCube(CellToWorld(baseCell, 0.05f), new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));

        if (drawBlockedCells)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.55f);
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Cell c = new Cell(x, y);
                    if (GetCellState(c) == CellState.Blocked)
                        Gizmos.DrawCube(CellToWorld(c, 0.05f), new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
                }
            }
        }

        if (drawUnreachableCells)
        {
            Gizmos.color = new Color(1f, 1f, 0.2f, 0.25f);
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Cell c = new Cell(x, y);
                    if (GetCellState(c) == CellState.Empty && GetDistance(distanceToBase, c) == -1)
                        Gizmos.DrawCube(CellToWorld(c, 0.05f), new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
                }
            }
        }
    }
}
