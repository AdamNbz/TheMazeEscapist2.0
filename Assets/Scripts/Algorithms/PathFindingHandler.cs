using System.Collections.Generic;
using UnityEngine;

public class PathFindingHandler
{
    Dictionary<Vector3Int, TileType> gridGraph = new();
    Dictionary<Vector3Int, TileType> rawGrid = new();

    public void Init(Dictionary<Vector3Int, TileType> rawGrid)
    {
        this.rawGrid = rawGrid;
    }

    public List<Vector3Int> WalkableDirection(Vector3Int start)
    {
        List<Vector3Int> walkableDirections = new();

        Vector3Int[] directions = new Vector3Int[]
        {
                new(1, 0, 0), // right
                new(-1, 0, 0), // left
                new(0, 1, 0), // up
                new(0, -1, 0) // down
        };

        foreach (var dir in directions)
        {
            Vector3Int neighborPos = start + dir;
            if (rawGrid.ContainsKey(neighborPos) && rawGrid[neighborPos] != TileType.Wall)
                walkableDirections.Add(dir);
        }

        return walkableDirections;
    }
}