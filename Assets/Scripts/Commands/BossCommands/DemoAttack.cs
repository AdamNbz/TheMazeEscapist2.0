using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DemoAttack : BossCommand
{
    public DemoAttack(BossController boss) : base(boss) { }

    public override async Task Execute()
    {
        isExecuting = true;
        isCompleted = false;

        for (int i = 1; i < boss.size - 1; i++)
        {
            for (int j = 1; j < boss.size - 1; j++)
            {
                boss.TriggerRaisingWall(new Vector3Int((int)boss.originCell.x + i, (int)boss.originCell.y - j, 0));
            }
        }
        await UniTask.Delay(8000);

        // Test pencil attack on cell 0 0
        boss.TriggerPencilAttack(2f, 1f, 5f, new Vector3Int(1, 0, 0), boss.originCell);
        boss.TriggerPencilAttack(2f, 1f, 5f, new Vector3Int(0, 1, 0), boss.originCell);

        //wait 8 seconds then lower wall
        await UniTask.Delay(3000);

        for (int i = 1; i < boss.size - 1; i++)
        {
            for (int j = 1; j < boss.size - 1; j++)
            {
                boss.TriggerLoweringWall(new Vector3Int((int)boss.originCell.x + i, (int)boss.originCell.y - j, 0));
            }
        }

        await UniTask.Delay(2000);

        isExecuting = false;
        isCompleted = true;
    }
}
