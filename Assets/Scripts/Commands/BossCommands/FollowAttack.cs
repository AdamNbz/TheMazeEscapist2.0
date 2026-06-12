using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class FollowAttack : BossCommand
{
    public FollowAttack(BossController boss) : base(boss) { }
    List<Vector3Int> directions = new List<Vector3Int>() {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0)
    };
    public override async Task Execute()
    {
        Debug.Log("Executing Follow Attack Command");
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(5, 8);
        var playerObject = boss.playerObject;
        // Spawn warning tiles around the player
        var playerInitCell = GridManager.Instance.WorldToCell(playerObject.transform.position);

        foreach (var dir in directions)
        {
            var warningTileCell = playerInitCell + dir;
            boss.TriggerCreateTile(warningTileCell, boss.WarningTilePrefab);
        }
        for (int i = 0; i < attackCount; i++)
        {
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);
            boss.TriggerPencilAttack(2f, 1f, 15f, directions[i % directions.Count], playerCell);
            await UniTask.Delay(3000);
        }

        await UniTask.Delay(2000);

        isExecuting = false;
        isCompleted = true;
    }
}