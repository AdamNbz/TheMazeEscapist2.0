using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ChasingAttack : BossCommand
{
    public ChasingAttack(BossController boss) : base(boss) { }
    List<Vector3Int> directions = new List<Vector3Int>() {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0)
    };
    public override async Task Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(10, 15);
        var playerObject = boss.playerObject;
        var originCell = BossController.originCell;
        // Spawn warning tiles around the player

        for (int k = 0; k < attackCount; k++)
        {
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);
            boss.TriggerCreateTile(playerCell, boss.WarningTilePrefab);
            await UniTask.Delay(2000);
        }
        await UniTask.Delay(5000);

        isExecuting = false;
        isCompleted = true;
    }
}
