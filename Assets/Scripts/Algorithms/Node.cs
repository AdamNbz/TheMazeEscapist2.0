using UnityEngine;

public class Node
{
    public float G { get; set; }
    public float H { get; set; }
    public float F => G + H;
    public Vector3Int position;
    public TileType type;
    public SpecialTile specialTile = null; // only set if type is Special, null otherwise
    public Node connection;
}