using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    #region Singleton
    private static GridManager _instance;
    public static GridManager Instance { get { return _instance; } }

    [SerializeField] private GameObject raisableWallPrefab;

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
        SpecialTile.OnSpecialTileInteracted += HandleSpecialTileInteracted;
    }

    void OnDisable()
    {
        SpecialTile.OnSpecialTileInstantiated -= HandleSpecialTileInstantiated;
        SpecialTile.OnSpecialTileInteracted -= HandleSpecialTileInteracted;
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
        Vector3Int toCellPos;
        float stopTime = 0;

        while (true)
        {
            stopTime = 0; // reset stopTime before checking each cell
            toCellPos = startCellPos + (Vector3Int)direction;
            if (!IsWalkable(startCellPos, toCellPos, direction, ref stopTime))
                break;

            result.directions.Add(new NodeData(direction, stopTime));

            if (gridMap[startCellPos].type == TileType.Slime) // slime stops movement
                break;

            var prevDirection = -direction;
            var fourDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var countPossibleDirections = 0;
            foreach (var dir in fourDirections)
            {
                if (dir == prevDirection)
                    continue;

                var nextCellPos = toCellPos + (Vector3Int)dir;
                if (IsWalkable(toCellPos, nextCellPos, dir, ref stopTime))
                {
                    countPossibleDirections++;
                    direction = dir;
                }
            }

            if (countPossibleDirections != 1)
                break;
            startCellPos = toCellPos;
        }
        return result;
    }

    public bool IsWalkable(Vector3Int fromCellPos, Vector3Int toCellPos, Vector2 direction, ref float stopTime)
    {
        if (!gridMap.ContainsKey(toCellPos))
            return false;

        if (!gridMap.ContainsKey(fromCellPos))
            return false;

        // Check if the toCellPos is a ladder
        if (gridMap[toCellPos].specialTile != null && gridMap[toCellPos].specialTile.Type == TileType.Ladder)
        {
            var ladder = gridMap[toCellPos].specialTile as Ladder;
            Debug.Log("Ladder: " + ladder.CanGoIn(direction).ToString() + " direction: " + direction.ToString());
            if (ladder.CanGoIn(direction))
                return true;
            else
                return false;
        }

        // Check if the fromCellPos is a ladder
        if (gridMap[fromCellPos].specialTile != null && gridMap[fromCellPos].specialTile.Type == TileType.Ladder)
        {
            var ladder = gridMap[fromCellPos].specialTile as Ladder;
            if (ladder.CanGoIn(direction))
                return true;
            else
                return false;
        }

        // Check if the toCellPos and fromCellPos are walkable or wall,
        // if one of them is walkable and the other is wall, return false
        TileType toTileType = gridMap[toCellPos].type;
        TileType fromTileType = gridMap[fromCellPos].type;

        if ((toTileType == TileType.Walkable && fromTileType == TileType.Wall) ||
            (toTileType == TileType.Wall && fromTileType == TileType.Walkable))
            return false;

        // Check if the toCellPos is a one way door
        if (gridMap[toCellPos].specialTile != null && gridMap[toCellPos].specialTile.Type == TileType.OneWayDoor)
        {
            var oneWayDoor = gridMap[toCellPos].specialTile as OneWayDoor;
            if (oneWayDoor.CanGoThrough(direction))
            {
                stopTime = oneWayDoor.StopTime;
                return true;
            }
            else
                return false;
        }

        return true;
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
        //gridMap[WorldToCell(tile.transform.position)].type = tile.Type;
        gridMap[WorldToCell(tile.transform.position)].specialTile = tile;
    }

    public bool IsNodeInteractable(Vector3Int cellPos)
    {
        return gridMap.ContainsKey(cellPos);
    }

    private void HandleSpecialTileInteracted(SpecialTile tile)
    {
        if (tile.Type == TileType.Rock)
        {
            Vector3Int cellPos = wallTilemap.WorldToCell(tile.transform.position);
            gridMap[cellPos].type = TileType.Wall;
        }
    }

    // Hàm này dùng để debug loại tile và special tile khi click chu?t, có thể bỏ qua nếu không cần
    //private void Update()
    //{
    //    // 1. Ki?m tra click chu?t theo New Input System (t??ng ???ng GetMouseButtonDown)
    //    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
    //    {
    //        // 2. L?y v? trí chu?t trên màn hình theo New Input System (t??ng ???ng Input.mousePosition)
    //        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

    //        // 3. Chuy?n ??i sang t?a ?? th? gi?i thông qua Camera
    //        // T?o m?t Vector3 t?m th?i v?i Z phù h?p ?? Camera.ScreenToWorldPoint tính toán ?úng
    //        Vector3 screenPosWithZ = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z));
    //        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPosWithZ);

    //        // 4. D?ch t?a ?? th? gi?i sang t?a ?? ô l??i (Cell Position)
    //        Vector3Int cellPos = WorldToCell(mouseWorldPos);

    //        // 5. Ki?m tra d? li?u trong gridMap c?a b?n
    //        if (gridMap != null && gridMap.TryGetValue(cellPos, out var cell))
    //        {
    //            string specialType = cell.specialTile != null ? cell.specialTile.Type.ToString() : "None";
    //            Debug.Log($"[CLICK] Ô vuông: {cell.position} | Lo?i ??t: {cell.type} | ??c bi?t: {specialType}");
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"B?n v?a click vào ô {cellPos}, nh?ng ô này không n?m trong d? li?u gridMap!");
    //        }
    //    }
    //}
    public void SetNodeType(Vector3Int cellPos, TileType type)
    {
        if (gridMap.ContainsKey(cellPos))
        {
            gridMap[cellPos].type = type;
        }
    }

    public void RaiseWall(Vector3Int cellPos)
    {
        if (gridMap.ContainsKey(cellPos) && gridMap[cellPos].type != TileType.Wall)
        {
            // Instantiate a raisable wall at the given cell position
            var worldPos = CellToWorld(cellPos);
            var wallObj = Instantiate(raisableWallPrefab, worldPos + new Vector3(0.5f, 0f, 0f), Quaternion.identity);
            var wallTile = wallObj.GetComponent<RaisableWall>();
            if (wallTile != null)
            {
                wallTile.Raise();
                gridMap[cellPos].type = TileType.Wall;
                gridMap[cellPos].specialTile = wallTile;
            }
        }
    }

    public void LowerWall(Vector3Int cellPos)
    {
        if (gridMap.ContainsKey(cellPos) && gridMap[cellPos].type == TileType.Wall && gridMap[cellPos].specialTile != null)
        {
            var wallTile = gridMap[cellPos].specialTile.GetComponent<RaisableWall>();
            if (wallTile != null)
            {
                wallTile.Lower();
                // grid map will set to walkable after wall finishes lowering
            }
        }
    }
}
