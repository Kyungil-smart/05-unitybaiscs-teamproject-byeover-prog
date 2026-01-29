using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Tilemap을 스캔해서 GridSystem에 Blocked Cell로 등록
/// </summary>
[RequireComponent(typeof(Tilemap))]
public sealed class TilemapBlockerScanner : MonoBehaviour
{
    [SerializeField] private GridSystem gridSystem;
    [SerializeField] private Tilemap obstacleTilemap;

    private void Awake()
    {
        if (gridSystem == null)
            gridSystem = FindObjectOfType<GridSystem>();

        if (obstacleTilemap == null)
            obstacleTilemap = GetComponent<Tilemap>();

        ScanAndApply();
    }

    [ContextMenu("Rescan Tilemap")]
    public void ScanAndApply()
    {
        BoundsInt bounds = obstacleTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (!obstacleTilemap.HasTile(pos))
                continue;

            // Tilemap 좌표 → 월드 → Grid Cell
            Vector3 world = obstacleTilemap.GetCellCenterWorld(pos);
            Cell cell = gridSystem.WorldToCell(world);

            if (gridSystem.IsInside(cell))
            {
                // 타워, 몬스터, 플레이어 모두 못지나가게 막음
                gridSystem.TryPlaceTower(cell); // 길막 검증도 같이 수행
            }
        }
    }
}
