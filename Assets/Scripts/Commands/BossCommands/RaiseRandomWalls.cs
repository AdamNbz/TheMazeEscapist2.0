using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RaiseRandomWalls : BossCommand
{
    public RaiseRandomWalls(BossController boss) : base(boss) { }
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

        for (int i = 0; i < BossController.size * BossController.size / 4; i++)
        {
            var randomCell = new Vector3Int(Random.Range((int)BossController.originCell.x, (int)BossController.originCell.x + BossController.size), Random.Range((int)BossController.originCell.y - BossController.size, (int)BossController.originCell.y), 0);
            boss.TriggerRaisingWall(randomCell);
        }
        await UniTask.Delay(8000);

        isExecuting = false;
        isCompleted = true;
    }
}