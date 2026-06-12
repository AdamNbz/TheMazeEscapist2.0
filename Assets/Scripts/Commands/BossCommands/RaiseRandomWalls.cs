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

        for (int i = 0; i < boss.size * boss.size / 4; i++)
        {
            var randomCell = new Vector3Int(Random.Range((int)boss.originCell.x, (int)boss.originCell.x + boss.size), Random.Range((int)boss.originCell.y - boss.size, (int)boss.originCell.y), 0);
            boss.TriggerRaisingWall(randomCell);
        }
        await UniTask.Delay(8000);

        isExecuting = false;
        isCompleted = true;
    }
}