using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OneWayDoor : SpecialTile
{
    [Tooltip("The direction from which the door can be passed through")]
    [SerializeField] Vector2 direction; // The direction from which the door can be passed through

    public override TileType Type => TileType.OneWayDoor;

    void Start()
    {
        OnInstantiated();
    }

    public bool CanGoThrough(Vector2 moveDirection)
    {
        return moveDirection == direction;
    }
}
