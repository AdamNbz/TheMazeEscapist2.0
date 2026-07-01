using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class RotatableTerrainBlock : MonoBehaviour
{
    [SerializeField] private Transform rotatingRoot;
    [SerializeField] private Transform pivot;
    [SerializeField] private Tilemap rotatableTilemap;
    [SerializeField] private float rotationDuration = 0.35f;
    [SerializeField] private bool updateGridBeforeAnimation = true;
    [SerializeField] private bool registerInitialWallsOnStart = true;
    [SerializeField] private bool useTilemapBoundsCenterAsPivot = true;
    [SerializeField] private bool requirePlayerOnPivotWhenInsideTerrain = true;
    [SerializeField] private PlayerController playerRequiredForRotation;

    private readonly HashSet<Vector3Int> currentBlockingCells = new();
    private bool hasCachedTilemapPivot;
    private Vector3 cachedTilemapPivotCell;
    private Vector3 cachedTilemapPivotWorld;

    public event UnityAction<RotatableTerrainBlock> RotationStarted;
    public event UnityAction<RotatableTerrainBlock> RotationFinished;

    public bool IsRotating { get; private set; }

    private void Reset()
    {
        rotatableTilemap = GetComponent<Tilemap>();
    }

    private void Awake()
    {
        if (rotatableTilemap == null)
            rotatableTilemap = GetComponent<Tilemap>();

        if (rotatingRoot == null)
            rotatingRoot = rotatableTilemap != null ? rotatableTilemap.transform : transform;

        if (pivot == null)
            pivot = transform;
    }

    private void Start()
    {
        CacheCurrentBlockingCells();

        if (registerInitialWallsOnStart)
            SetCellsType(currentBlockingCells, TileType.Wall);
    }

    public bool TryRotateClockwise()
    {
        return TryRotateClockwiseTurns(1);
    }

    public bool TryRotateCounterClockwise()
    {
        return TryRotateClockwiseTurns(-1);
    }

    public bool TryRotateClockwiseTurns(int quarterTurns)
    {
        var normalizedTurns = NormalizeQuarterTurns(quarterTurns);
        if (IsRotating || normalizedTurns == 0)
            return false;

        if (rotatableTilemap == null)
        {
            Debug.LogWarning($"{nameof(RotatableTerrainBlock)} needs a Tilemap assigned.", this);
            return false;
        }

        if (!CanRotateWithPlayer(normalizedTurns))
            return false;

        StartCoroutine(RotateRoutine(normalizedTurns));
        return true;
    }

    private bool CanRotateWithPlayer(int clockwiseQuarterTurns)
    {
        if (!TryGetPlayerCell(out var playerCell))
            return true;

        if (requirePlayerOnPivotWhenInsideTerrain && IsCellInsideCurrentTerrainBounds(playerCell) && playerCell != GetPivotGridCell())
            return false;

        return !GetRotatedBlockingCells(clockwiseQuarterTurns).Contains(playerCell);
    }

    private bool IsCellInsideCurrentTerrainBounds(Vector3Int cellPos)
    {
        if (currentBlockingCells.Count == 0)
            return false;

        var minX = int.MaxValue;
        var maxX = int.MinValue;
        var minY = int.MaxValue;
        var maxY = int.MinValue;
        var minZ = int.MaxValue;
        var maxZ = int.MinValue;

        foreach (var blockingCell in currentBlockingCells)
        {
            minX = Mathf.Min(minX, blockingCell.x);
            maxX = Mathf.Max(maxX, blockingCell.x);
            minY = Mathf.Min(minY, blockingCell.y);
            maxY = Mathf.Max(maxY, blockingCell.y);
            minZ = Mathf.Min(minZ, blockingCell.z);
            maxZ = Mathf.Max(maxZ, blockingCell.z);
        }

        return cellPos.x >= minX && cellPos.x <= maxX
            && cellPos.y >= minY && cellPos.y <= maxY
            && cellPos.z >= minZ && cellPos.z <= maxZ;
    }

    private bool TryGetPlayerCell(out Vector3Int playerCell)
    {
        playerCell = default;

        if (GridManager.Instance == null)
            return false;

        if (playerRequiredForRotation == null)
            playerRequiredForRotation = FindFirstObjectByType<PlayerController>();

        if (playerRequiredForRotation == null)
            return false;

        playerCell = GridManager.Instance.WorldToCell(playerRequiredForRotation.transform.position);
        return true;
    }

    private Vector3Int GetPivotGridCell()
    {
        var pivotCell = GetPivotCell();
        return new Vector3Int(
            Mathf.RoundToInt(pivotCell.x),
            Mathf.RoundToInt(pivotCell.y),
            Mathf.RoundToInt(pivotCell.z));
    }

    private IEnumerator RotateRoutine(int clockwiseQuarterTurns)
    {
        IsRotating = true;
        RotationStarted?.Invoke(this);

        var tileSnapshots = CollectTilemapSnapshots();
        var nextBlockingCells = GetRotatedBlockingCells(clockwiseQuarterTurns);
        if (updateGridBeforeAnimation)
            ApplyGridTransition(nextBlockingCells);

        yield return AnimateTilemapRotationAndBake(tileSnapshots, clockwiseQuarterTurns);

        if (!updateGridBeforeAnimation)
            ApplyGridTransition(nextBlockingCells);

        CacheCurrentBlockingCells();
        IsRotating = false;
        RotationFinished?.Invoke(this);
    }

    private void CacheCurrentBlockingCells()
    {
        currentBlockingCells.Clear();

        if (GridManager.Instance == null)
        {
            Debug.LogWarning($"{nameof(RotatableTerrainBlock)} needs a GridManager in the scene.", this);
            return;
        }

        if (rotatableTilemap != null)
        {
            var tileSnapshots = CollectTilemapSnapshots();
            CacheTilemapPivotIfNeeded(tileSnapshots);

            foreach (var tileSnapshot in tileSnapshots)
                currentBlockingCells.Add(tileSnapshot.GridCell);

            return;
        }
    }

    private HashSet<Vector3Int> GetRotatedBlockingCells(int clockwiseQuarterTurns)
    {
        var result = new HashSet<Vector3Int>();

        if (GridManager.Instance == null)
            return result;

        var pivotCell = GetPivotCell();
        foreach (var cellPos in currentBlockingCells)
            result.Add(RotateCellClockwise(cellPos, pivotCell, clockwiseQuarterTurns));

        return result;
    }

    private void ApplyGridTransition(HashSet<Vector3Int> nextBlockingCells)
    {
        foreach (var cellPos in currentBlockingCells)
        {
            if (!nextBlockingCells.Contains(cellPos))
                TrySetGridCell(cellPos, TileType.Walkable);
        }

        SetCellsType(nextBlockingCells, TileType.Wall);
        currentBlockingCells.Clear();

        foreach (var cellPos in nextBlockingCells)
            currentBlockingCells.Add(cellPos);
    }

    private void SetCellsType(IEnumerable<Vector3Int> cellPositions, TileType type)
    {
        foreach (var cellPos in cellPositions)
            TrySetGridCell(cellPos, type);
    }

    private bool TrySetGridCell(Vector3Int cellPos, TileType type)
    {
        if (GridManager.Instance == null)
            return false;

        var didSet = GridManager.Instance.TrySetNodeType(cellPos, type);
        if (!didSet)
            Debug.LogWarning($"{nameof(RotatableTerrainBlock)} tried to set {cellPos} to {type}, but the cell is not in GridManager.", this);

        return didSet;
    }

    private IEnumerator AnimateTilemapRotationAndBake(List<TileSnapshot> tileSnapshots, int clockwiseQuarterTurns)
    {
        if (tileSnapshots == null || tileSnapshots.Count == 0)
            yield break;

        var root = rotatingRoot != null ? rotatingRoot : rotatableTilemap.transform;
        var startPosition = root.position;
        var startRotation = root.rotation;
        var pivotWorld = GetPivotWorldPosition();
        var targetAngle = -90f * clockwiseQuarterTurns;

        if (rotationDuration > 0f)
        {
            var elapsed = 0f;
            var previousAngle = 0f;

            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / rotationDuration);
                t = t * t * (3f - 2f * t);
                var angle = Mathf.Lerp(0f, targetAngle, t);
                root.RotateAround(pivotWorld, Vector3.forward, angle - previousAngle);
                previousAngle = angle;

                yield return null;
            }

            root.RotateAround(pivotWorld, Vector3.forward, targetAngle - previousAngle);
        }

        root.SetPositionAndRotation(startPosition, startRotation);
        ApplyTilemapRotation(tileSnapshots, clockwiseQuarterTurns);
    }

    private void ApplyTilemapRotation(List<TileSnapshot> tileSnapshots, int clockwiseQuarterTurns)
    {
        var pivotCell = GetPivotCell();

        foreach (var tileSnapshot in tileSnapshots)
            rotatableTilemap.SetTile(tileSnapshot.TilemapCell, null);

        foreach (var tileSnapshot in tileSnapshots)
        {
            var rotatedGridCell = RotateCellClockwise(tileSnapshot.GridCell, pivotCell, clockwiseQuarterTurns);
            var targetTilemapCell = GridCellToTilemapCell(rotatedGridCell);

            rotatableTilemap.SetTile(targetTilemapCell, tileSnapshot.Tile);
            rotatableTilemap.SetTileFlags(targetTilemapCell, TileFlags.None);
            rotatableTilemap.SetColor(targetTilemapCell, tileSnapshot.Color);
            rotatableTilemap.SetTransformMatrix(targetTilemapCell, tileSnapshot.TransformMatrix);
            rotatableTilemap.SetTileFlags(targetTilemapCell, tileSnapshot.Flags);
        }

        rotatableTilemap.CompressBounds();
    }

    private List<TileSnapshot> CollectTilemapSnapshots()
    {
        var tileSnapshots = new List<TileSnapshot>();
        if (rotatableTilemap == null || GridManager.Instance == null)
            return tileSnapshots;

        foreach (var tilemapCell in rotatableTilemap.cellBounds.allPositionsWithin)
        {
            var tile = rotatableTilemap.GetTile(tilemapCell);
            if (tile == null)
                continue;

            tileSnapshots.Add(new TileSnapshot
            {
                GridCell = GridManager.Instance.WorldToCell(rotatableTilemap.GetCellCenterWorld(tilemapCell)),
                TilemapCell = tilemapCell,
                Tile = tile,
                Color = rotatableTilemap.GetColor(tilemapCell),
                TransformMatrix = rotatableTilemap.GetTransformMatrix(tilemapCell),
                Flags = rotatableTilemap.GetTileFlags(tilemapCell)
            });
        }

        return tileSnapshots;
    }

    private Vector3Int GridCellToTilemapCell(Vector3Int gridCell)
    {
        return GridManager.Instance != null
            ? rotatableTilemap.WorldToCell(GridManager.Instance.GetCellCenterWorld(gridCell))
            : gridCell;
    }

    private Vector3 GetPivotCell()
    {
        if (rotatableTilemap != null && useTilemapBoundsCenterAsPivot)
        {
            CacheTilemapPivotIfNeeded(CollectTilemapSnapshots());
            if (hasCachedTilemapPivot)
                return cachedTilemapPivotCell;
        }

        return GridManager.Instance != null && pivot != null
            ? GridManager.Instance.WorldToCell(pivot.position)
            : Vector3.zero;
    }

    private Vector3 GetPivotWorldPosition()
    {
        if (rotatableTilemap != null && useTilemapBoundsCenterAsPivot)
        {
            CacheTilemapPivotIfNeeded(CollectTilemapSnapshots());
            if (hasCachedTilemapPivot)
                return cachedTilemapPivotWorld;
        }

        return pivot != null ? pivot.position : transform.position;
    }

    private void CacheTilemapPivotIfNeeded(List<TileSnapshot> tileSnapshots)
    {
        if (hasCachedTilemapPivot || !useTilemapBoundsCenterAsPivot || tileSnapshots == null || tileSnapshots.Count == 0)
            return;

        var minX = tileSnapshots[0].GridCell.x;
        var maxX = tileSnapshots[0].GridCell.x;
        var minY = tileSnapshots[0].GridCell.y;
        var maxY = tileSnapshots[0].GridCell.y;
        var z = tileSnapshots[0].GridCell.z;

        foreach (var tileSnapshot in tileSnapshots)
        {
            minX = Mathf.Min(minX, tileSnapshot.GridCell.x);
            maxX = Mathf.Max(maxX, tileSnapshot.GridCell.x);
            minY = Mathf.Min(minY, tileSnapshot.GridCell.y);
            maxY = Mathf.Max(maxY, tileSnapshot.GridCell.y);
        }

        cachedTilemapPivotCell = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, z);

        var minTilemapCell = GridCellToTilemapCell(new Vector3Int(minX, minY, z));
        var maxTilemapCell = GridCellToTilemapCell(new Vector3Int(maxX, maxY, z));
        cachedTilemapPivotWorld = (rotatableTilemap.GetCellCenterWorld(minTilemapCell) + rotatableTilemap.GetCellCenterWorld(maxTilemapCell)) * 0.5f;
        hasCachedTilemapPivot = true;
    }

    private static Vector3Int RotateCellClockwise(Vector3Int cellPos, Vector3 pivotCell, int clockwiseQuarterTurns)
    {
        var offset = new Vector3(cellPos.x - pivotCell.x, cellPos.y - pivotCell.y, cellPos.z - pivotCell.z);
        for (var i = 0; i < clockwiseQuarterTurns; i++)
            offset = new Vector3(offset.y, -offset.x, offset.z);

        return new Vector3Int(
            Mathf.RoundToInt(pivotCell.x + offset.x),
            Mathf.RoundToInt(pivotCell.y + offset.y),
            Mathf.RoundToInt(pivotCell.z + offset.z));
    }

    private static int NormalizeQuarterTurns(int quarterTurns)
    {
        quarterTurns %= 4;
        if (quarterTurns < 0)
            quarterTurns += 4;

        return quarterTurns;
    }

    private struct TileSnapshot
    {
        public Vector3Int GridCell { get; set; }
        public Vector3Int TilemapCell { get; set; }
        public TileBase Tile { get; set; }
        public Color Color { get; set; }
        public Matrix4x4 TransformMatrix { get; set; }
        public TileFlags Flags { get; set; }
    }
}
