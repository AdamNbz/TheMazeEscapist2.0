using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    #region Singleton
    private static GridManager _instance;
    public static GridManager Instance { get { return _instance; } }
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    
        foreach (var pos in walkableTilemap.cellBounds.allPositionsWithin)
        {
            // Do something with each position
            gridMap[pos] = new Node { position = pos, type = TileType.Walkable };
            if (wallTilemap.HasTile(pos))
            {
                gridMap[pos] = new Node { position = pos, type = TileType.Wall };
            }
        }
    }
    #endregion
    
    [SerializeField] Tilemap walkableTilemap;
    [SerializeField] Tilemap wallTilemap;
    [SerializeField] Grid grid;

    Dictionary<Vector3Int, Node> gridMap = new(); //true for walkable, false for wall

    public Dictionary<Vector3Int, Node> GetGrid()
    {
        return gridMap;
    }

    void OnEnable()
    {
        SpecialTile.OnSpecialTileInstantiated += HandleSpecialTileInstantiated;
        Rock.OnRockEnabled += HandleRockEnabled;
    }

    void OnDisable()
    {
        SpecialTile.OnSpecialTileInstantiated -= HandleSpecialTileInstantiated;
        Rock.OnRockEnabled -= HandleRockEnabled;
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public Path FindPathFromWorld(Vector3 startWorldPos, Vector2Int direction)
    {
        Vector3Int startCellPos = grid.WorldToCell(startWorldPos);
        return FindPathFromCell(startCellPos, direction);
    }

    public Path FindPathFromCell(Vector3Int startCellPos, Vector2Int direction)
    {
        var result = new Path
        {
            stepLength = grid.transform.localScale.x
        };

        while (true)
        {
            startCellPos += (Vector3Int)direction;
            if (!IsWalkable(startCellPos))
                break;

            result.directions.Add(direction);

            var prevDirection = -direction;
            var fourDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var countPossibleDirections = 0;
            foreach (var dir in fourDirections)
            {
                if (dir == prevDirection)
                    continue;

                var nextCellPos = startCellPos + (Vector3Int)dir;
                if (IsWalkable(nextCellPos))
                {
                    countPossibleDirections++;
                    direction = dir;
                }
            }

            if (countPossibleDirections != 1)
                break;
        }
        return result;
    }

    public bool IsWalkable(Vector3Int cellPos)
    {
        return gridMap.ContainsKey(cellPos) && gridMap[cellPos].type != TileType.Wall;
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return grid.WorldToCell(worldPos);
    }

    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        return grid.CellToWorld(cellPos);
    }

    private void HandleSpecialTileInstantiated(SpecialTile tile)
    {
        gridMap[WorldToCell(tile.transform.position)].type = tile.Type;
        gridMap[WorldToCell(tile.transform.position)].specialTile = tile;
    }

    public bool IsNodeInteractable(Vector3Int cellPos)
    {
        return gridMap.ContainsKey(cellPos);
    }

    private void HandleRockEnabled(Vector3 rockPosition)
    {
        Vector3Int cellPos = wallTilemap.WorldToCell(rockPosition);
        gridMap[cellPos].type = TileType.Wall;
    }
}
