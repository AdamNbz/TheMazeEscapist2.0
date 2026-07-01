using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class RainbowTilemapColor : MonoBehaviour
{
    [SerializeField] private Tilemap[] targetTilemaps;
    [SerializeField] private bool searchChildTilemapsIfNeeded = true;
    [SerializeField] private bool colorEachTile = true;
    [SerializeField, Min(0f)] private float cycleSpeed = 0.35f;
    [SerializeField, Range(0f, 1f)] private float saturation = 1f;
    [SerializeField, Range(0f, 1f)] private float value = 1f;
    [SerializeField] private Vector2 cellHueOffset = new(0.08f, 0.08f);
    [SerializeField] private bool restoreOriginalColorsOnDisable = true;

    private readonly List<TilemapState> tilemapStates = new();

    private void Reset()
    {
        targetTilemaps = FindTargetTilemaps();
    }

    private void Awake()
    {
        if (targetTilemaps == null || targetTilemaps.Length == 0)
            targetTilemaps = FindTargetTilemaps();
    }

    private void OnEnable()
    {
        CacheTilemaps();
        ApplyRainbowColors();
    }

    private void OnDisable()
    {
        if (restoreOriginalColorsOnDisable)
            RestoreOriginalColors();
    }

    private void Update()
    {
        ApplyRainbowColors();
    }

    private Tilemap[] FindTargetTilemaps()
    {
        var localTilemaps = GetComponents<Tilemap>();
        if (localTilemaps.Length > 0 || !searchChildTilemapsIfNeeded)
            return localTilemaps;

        return GetComponentsInChildren<Tilemap>();
    }

    private void CacheTilemaps()
    {
        tilemapStates.Clear();

        if (targetTilemaps == null || targetTilemaps.Length == 0)
            targetTilemaps = FindTargetTilemaps();

        foreach (var tilemap in targetTilemaps)
        {
            if (tilemap == null)
                continue;

            tilemapStates.Add(new TilemapState(tilemap));
        }
    }

    private void ApplyRainbowColors()
    {
        if (tilemapStates.Count == 0)
            CacheTilemaps();

        var baseHue = Mathf.Repeat(Time.time * cycleSpeed, 1f);

        foreach (var state in tilemapStates)
        {
            if (state.Tilemap == null)
                continue;

            if (colorEachTile)
            {
                ApplyCellRainbow(state, baseHue);
                continue;
            }

            var color = Color.HSVToRGB(baseHue, saturation, value);
            color.a = state.OriginalTilemapColor.a;
            state.Tilemap.color = color;
        }
    }

    private void ApplyCellRainbow(TilemapState state, float baseHue)
    {
        foreach (var cell in state.Tilemap.cellBounds.allPositionsWithin)
        {
            if (!state.Tilemap.HasTile(cell))
                continue;

            var originalCell = state.GetOrAddOriginalCell(cell);
            var hue = Mathf.Repeat(baseHue + cell.x * cellHueOffset.x + cell.y * cellHueOffset.y, 1f);
            var color = Color.HSVToRGB(hue, saturation, value);
            color.a = originalCell.Color.a;

            state.Tilemap.SetTileFlags(cell, originalCell.Flags & ~TileFlags.LockColor);
            state.Tilemap.SetColor(cell, color);
        }
    }

    private void RestoreOriginalColors()
    {
        foreach (var state in tilemapStates)
        {
            if (state.Tilemap == null)
                continue;

            state.Tilemap.color = state.OriginalTilemapColor;

            foreach (var originalCell in state.OriginalCells)
            {
                if (!state.Tilemap.HasTile(originalCell.Key))
                    continue;

                state.Tilemap.SetTileFlags(originalCell.Key, originalCell.Value.Flags & ~TileFlags.LockColor);
                state.Tilemap.SetColor(originalCell.Key, originalCell.Value.Color);
                state.Tilemap.SetTileFlags(originalCell.Key, originalCell.Value.Flags);
            }
        }
    }

    private sealed class TilemapState
    {
        public TilemapState(Tilemap tilemap)
        {
            Tilemap = tilemap;
            OriginalTilemapColor = tilemap.color;
        }

        public Tilemap Tilemap { get; }
        public Color OriginalTilemapColor { get; }
        public Dictionary<Vector3Int, CellColorState> OriginalCells { get; } = new();

        public CellColorState GetOrAddOriginalCell(Vector3Int cell)
        {
            if (OriginalCells.TryGetValue(cell, out var originalCell))
                return originalCell;

            originalCell = new CellColorState(Tilemap.GetColor(cell), Tilemap.GetTileFlags(cell));
            OriginalCells[cell] = originalCell;
            return originalCell;
        }
    }

    private readonly struct CellColorState
    {
        public CellColorState(Color color, TileFlags flags)
        {
            Color = color;
            Flags = flags;
        }

        public Color Color { get; }
        public TileFlags Flags { get; }
    }
}
