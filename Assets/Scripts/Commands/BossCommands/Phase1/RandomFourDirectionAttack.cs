using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class RandomFourDirectionAttack : BossCommand
{
    public RandomFourDirectionAttack(BossController boss) : base(boss) { }
    public override async Task Execute()
    {
        isExecuting = true;
        isCompleted = false;

        //Get player's position
        var attackCount = Random.Range(4, 7);
        var playerObject = boss.playerObject;
        var originCell = BossController.originCell;
        // Spawn warning tiles around the player

        for (int k = 0; k < attackCount; k++)
        {
            var randomDirection = directions[Random.Range(0, directions.Count)];
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);

            boss.TriggerPencilAttack(0.5f, 1.0f, 10f, randomDirection, playerCell);

            await UniTask.Delay(2000);
        }
        await UniTask.Delay(5000);

        isExecuting = false;
        isCompleted = true;
    }
}
