using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RotatableTerrainBlock : MonoBehaviour
{
    [SerializeField] private Transform rotatingRoot;
    [SerializeField] private Transform pivot;
    [SerializeField] private List<RotatableTerrainCell> cells = new();
    [SerializeField] private float rotationDuration = 0.35f;
    [SerializeField] private bool updateGridBeforeAnimation = true;
    [SerializeField] private bool registerInitialWallsOnStart = true;

    private readonly HashSet<Vector3Int> currentBlockingCells = new();

    public event UnityAction<RotatableTerrainBlock> RotationStarted;
    public event UnityAction<RotatableTerrainBlock> RotationFinished;

    public bool IsRotating { get; private set; }

    private void Awake()
    {
        if (rotatingRoot == null)
            rotatingRoot = transform;

        if (pivot == null)
            pivot = transform;

        AutoCollectCellsIfNeeded();
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

        StartCoroutine(RotateRoutine(normalizedTurns));
        return true;
    }

    private IEnumerator RotateRoutine(int clockwiseQuarterTurns)
    {
        IsRotating = true;
        RotationStarted?.Invoke(this);

        var nextBlockingCells = GetRotatedBlockingCells(clockwiseQuarterTurns);
        if (updateGridBeforeAnimation)
            ApplyGridTransition(nextBlockingCells);

        var startRotation = rotatingRoot.eulerAngles;
        var targetRotation = startRotation + new Vector3(0f, 0f, -90f * clockwiseQuarterTurns);
        var elapsed = 0f;

        if (rotationDuration <= 0f)
        {
            rotatingRoot.eulerAngles = targetRotation;
        }
        else
        {
            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / rotationDuration);
                t = t * t * (3f - 2f * t);
                rotatingRoot.eulerAngles = new Vector3(
                    Mathf.LerpAngle(startRotation.x, targetRotation.x, t),
                    Mathf.LerpAngle(startRotation.y, targetRotation.y, t),
                    Mathf.LerpAngle(startRotation.z, targetRotation.z, t));

                yield return null;
            }

            rotatingRoot.eulerAngles = targetRotation;
        }

        if (!updateGridBeforeAnimation)
            ApplyGridTransition(nextBlockingCells);

        CacheCurrentBlockingCells();
        IsRotating = false;
        RotationFinished?.Invoke(this);
    }

    private void AutoCollectCellsIfNeeded()
    {
        if (cells.Count > 0)
            return;

        foreach (var cell in GetComponentsInChildren<RotatableTerrainCell>(true))
        {
            if (cell != null && cell.BlocksMovement)
                cells.Add(cell);
        }
    }

    private void CacheCurrentBlockingCells()
    {
        currentBlockingCells.Clear();

        if (GridManager.Instance == null)
        {
            Debug.LogWarning($"{nameof(RotatableTerrainBlock)} needs a GridManager in the scene.", this);
            return;
        }

        foreach (var cell in cells)
        {
            if (cell == null || !cell.BlocksMovement)
                continue;

            currentBlockingCells.Add(GridManager.Instance.WorldToCell(cell.transform.position));
        }
    }

    private HashSet<Vector3Int> GetRotatedBlockingCells(int clockwiseQuarterTurns)
    {
        var result = new HashSet<Vector3Int>();

        if (GridManager.Instance == null)
            return result;

        var pivotCell = GridManager.Instance.WorldToCell(pivot.position);
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

    private static Vector3Int RotateCellClockwise(Vector3Int cellPos, Vector3Int pivotCell, int clockwiseQuarterTurns)
    {
        var offset = cellPos - pivotCell;
        for (var i = 0; i < clockwiseQuarterTurns; i++)
            offset = new Vector3Int(offset.y, -offset.x, offset.z);

        return pivotCell + offset;
    }

    private static int NormalizeQuarterTurns(int quarterTurns)
    {
        quarterTurns %= 4;
        if (quarterTurns < 0)
            quarterTurns += 4;

        return quarterTurns;
    }
}
