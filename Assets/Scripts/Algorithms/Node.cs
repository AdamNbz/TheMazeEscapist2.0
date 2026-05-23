using UnityEngine;

public class Node
{
    public Vector3Int position;
    public TileType type;
    public SpecialTile specialTile = null; // only set if type is Special, null otherwise
}