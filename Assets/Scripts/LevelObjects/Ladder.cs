using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Ladder : SpecialTile
{
    public override TileType Type => TileType.Ladder;

    public Vector2 goUpDirection; // Hướng mà player có thể đi qua ladder,
                                      // dùng để xác định hướng di chuyển của player khi tương tác
                                      // với ladder

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnInstantiated();
    }

    public bool CanGoIn(Vector2 moveDirection)
    {
        return moveDirection == goUpDirection || moveDirection == goUpDirection * -1;
    }
}
