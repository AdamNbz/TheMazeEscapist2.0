using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SnakeChaseAttack : BossCommand
{
    public SnakeChaseAttack(BossController boss) : base(boss) { }
    Vector3Int currentCell;
    Vector3Int currentDirection = Vector3Int.zero;
    public override async Task Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(30, 50);
        var playerObject = boss.playerObject;
        var originCell = BossController.originCell;

        currentCell = boss.RandomWalkableCell();
        var tileObject = boss.TriggerCreateTile(currentCell, boss.WarningTilePrefab);
        var warningTile = tileObject.GetComponent<WarningTile>();
        warningTile.Init(0.75f);

        await UniTask.Delay(700);

        for (int k = 0; k < attackCount; k++)
        {
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);
            Vector3Int nextCell = currentCell;
            int minDistance = int.MaxValue;
            foreach (var direction in directions)
            {
                var newCell = currentCell + direction;
                if (GridManager.Instance.IsWalkable(newCell) && direction != -currentDirection)
                {
                    int distance = Mathf.Abs(newCell.x - playerCell.x) + Mathf.Abs(newCell.y - playerCell.y);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nextCell = newCell;
                    }
                }
            }
            currentDirection = nextCell - currentCell;
            currentCell = nextCell;
            tileObject = boss.TriggerCreateTile(currentCell, boss.WarningTilePrefab);
            warningTile = tileObject.GetComponent<WarningTile>();
            warningTile.Init(0.75f);
            await UniTask.Delay(500);
        }
        await UniTask.Delay(5000);

        isExecuting = false;
        isCompleted = true;
    }
}

