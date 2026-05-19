using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFindingHandler
{
    Dictionary<Vector3Int, Node> gridNodes = new();

    readonly Vector3Int[] neighborDirs = { new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0), new(0, -1, 0) };

    public PathFindingHandler()
    {
        foreach (var kvp in GridManager.Instance.GetGrid())
        {
            gridNodes[kvp.Key] = new Node
            {
                position = kvp.Value.position,
                type = kvp.Value.type,
                specialTile = kvp.Value.specialTile
            };
        }
    }


    public IEnumerable<Node> WalkableNeighbor(Node node)
    {
        bool isFindPortal = false;
        if (node.type == TileType.Portal)
        {
            var portal = node.specialTile as Portal;
            if (portal != null)
            {
                var linkedPortal = portal.linkedPortal;
                if (linkedPortal != null)
                {
                    var linkedNode = gridNodes[GridManager.Instance.WorldToCell(linkedPortal.transform.position)];
                    if (linkedNode.connection == null && IsNodeWalkable(linkedNode.position))
                    {
                        isFindPortal = true;
                        yield return linkedNode;
                    }
                }
            }
        }
        if (!isFindPortal)
            foreach (var dir in neighborDirs)
            {
                Vector3Int neighborPos = node.position + dir;
                if (IsNodeWalkable(neighborPos))
                    yield return gridNodes[neighborPos];
            }
    }

    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        var toSearch = new List<Node> { startNode };
        var processed = new List<Node>();

        while (toSearch.Count > 0)
        {
            var current = toSearch[0];
            foreach (var node in toSearch)
                if (node.F < current.F || (node.F == current.F && node.H < current.H))
                    current = node;

            processed.Add(current);
            toSearch.Remove(current);

            if (current == targetNode)
            {
                var path = new List<Node>();
                while (current != null)
                {
                    path.Add(current);
                    current = current.connection;
                }
                path.Reverse();

                CleanUpNodes(toSearch);
                CleanUpNodes(processed);

                return path;
            }

            foreach (var neighbor in WalkableNeighbor(current).Where(n => !processed.Contains(n)))
            {
                var inSearch = toSearch.Contains(neighbor);
                var costToNeighbor = current.G + 1;

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.G = costToNeighbor;
                    neighbor.connection = current;

                    if (!inSearch)
                    {
                        neighbor.H = Vector3Int.Distance(neighbor.position, targetNode.position);
                        toSearch.Add(neighbor);
                    }
                }
            }
        }
        return null;
    }

    public List<Node> FindPathFromPlayer(Vector3 playerWorldPos, SpecialTile targetTile)
    {
        var startNode = gridNodes[GridManager.Instance.WorldToCell(playerWorldPos)];
        var targetNode = gridNodes[GridManager.Instance.WorldToCell(targetTile.transform.position)];

        Debug.Log($"Finding path from {startNode.position} to {targetNode.position}");
        return FindPath(startNode, targetNode);
        // return new List<Node>();
    }

    private bool IsNodeWalkable(Vector3Int pos)
    {
        return gridNodes.ContainsKey(pos) && gridNodes[pos].type != TileType.Wall;
    }

    private void CleanUpNodes(List<Node> nodes)
    {
        foreach (var node in nodes)
        {
            node.G = float.MaxValue;
            node.H = 0;
            node.connection = null;
        }
    }
}