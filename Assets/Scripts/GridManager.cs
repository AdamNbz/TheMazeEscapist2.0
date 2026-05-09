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
    }
    #endregion
    [SerializeField] Tilemap walkableTilemap;
    [SerializeField] Tilemap wallTilemap;
    [SerializeField] Grid grid;

    Dictionary<Vector3Int, bool> gridMap = new Dictionary<Vector3Int, bool>(); //true for walkable, false for wall

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var pos in walkableTilemap.cellBounds.allPositionsWithin)
        {
            // Do something with each position
            gridMap[pos] = true;
            if (wallTilemap.HasTile(pos))
            {
                gridMap[pos] = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
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
            if (!gridMap.ContainsKey(startCellPos) || !gridMap[startCellPos])
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
                if (gridMap.ContainsKey(nextCellPos) && gridMap[nextCellPos])
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
}
