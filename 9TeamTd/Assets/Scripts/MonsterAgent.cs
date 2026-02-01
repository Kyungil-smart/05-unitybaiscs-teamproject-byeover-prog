using UnityEngine;
using UnityEngine.Serialization;

public sealed class MonsterAgent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridSystem gridSystem;

    // 속도는 Json으로 데이터 받아와서 필요없긴 한데, 디버깅용으로 남겨줌
    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
    
    [SerializeField, Min(0.001f)] private float arrivalRadius = 0.05f;

    [SerializeField, Min(0f)] private float turnSpeed = 12f;

    private Cell currentCell;
    private Cell targetCell;
    
    private Vector3 targetWorld;
    private Vector2Int lastMoveDir;
    
    private bool hasTarget;
    private bool isInitialized = false; // 초기화 여부 체크

    private void Awake()
    { 
        if (GridSystem.Instance != null) gridSystem = GridSystem.Instance;
        else gridSystem = FindObjectOfType<GridSystem>();
    }

    /*
    private void Start()
    {
        if (!TryGetSpawnCell(out Cell spawnCell))
        {
            enabled = false;
            return;
        }

        TeleportToCell(spawnCell);
    }
    */
    // Start 이벤트 함수 안쓰고 Initialize 함수를 매니저에서 호출해서 사용
    public void Initialize(float speed, Transform baseTarget)
    {
        this.moveSpeed = speed;
        
        // 현재 내 위치를 그리드 좌표로 인식
        if (gridSystem != null)
        {
            TeleportToCell(gridSystem.WorldToCell(baseTarget.position));
        }
        
        this.isInitialized = true;
    }

    private void Update()
    {
        if (!hasTarget || HasArrivedToTarget())
        {
            SnapToTarget();

            currentCell = targetCell;
            hasTarget = false;

            // 다음 이동할 곳 찾기
            if (!gridSystem.TryGetNextStep(currentCell, lastMoveDir, out Cell nextCell, out Vector2Int nextDir))
                return; // 없거나 도착하면 return

            targetCell = nextCell;
            lastMoveDir = nextDir;
            targetWorld = gridSystem.CellToWorld(nextCell, y: transform.position.y);
            hasTarget = true;
        }

        // 이동 (moveTowards)
        transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);

        // 회전 (LookRotation)
        Vector3 dir = targetWorld - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, Time.deltaTime * turnSpeed);
        }
    }
    
    /* MonsterManager에서 스폰 지정해주기 때문에 필요 없음 함수 이름 : TryGetSpawnCell
    private bool TryGetSpawnCell(out Cell spawnCell)
    {
        if (!gridSystem.TryGetRandomSpawnCell(out spawnCell))
        {
            Debug.LogWarning("[MonsterAgent] No reachable edge spawn cell.");
            return false;
        }

        return true;
    }
    */
    
    private void TeleportToCell(Cell cell)
    {
        currentCell = cell;
        targetCell = cell;

        if (gridSystem != null)
        {
            targetWorld = gridSystem.CellToWorld(cell, y: transform.position.y);
        }
        
        transform.position = targetWorld;
        hasTarget = false;
        lastMoveDir = Vector2Int.zero;
    }

    private bool HasArrivedToTarget()
    {
        float radiusSqr = arrivalRadius * arrivalRadius;
        return (transform.position - targetWorld).sqrMagnitude <= radiusSqr;
    }

    private void SnapToTarget()
    {
        transform.position = targetWorld;
    }
}
