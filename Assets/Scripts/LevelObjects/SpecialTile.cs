using UnityEngine;
using UnityEngine.Events;

public abstract class SpecialTile : MonoBehaviour
{
    public static UnityAction<SpTileData> OnSpecialTileInstantiated;
    public abstract TileType Type { get; }
    protected virtual void OnInstantiated()
    {
        OnSpecialTileInstantiated?.Invoke(new SpTileData
        {
            Position = transform.position,
            Type = Type
        });
    }

}

public struct SpTileData
{
    public Vector3 Position;
    public TileType Type;
}

public enum TileType
{
    Wall,
    Walkable,
    Portal,
    Trash,
    RecycleBin,
    StudentCard,
    WinPoint
}