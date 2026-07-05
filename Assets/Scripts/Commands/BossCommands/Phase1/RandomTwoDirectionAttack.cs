using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTwoDirectionAttack : BossCommand
{
    public RandomTwoDirectionAttack(BossController boss) : base(boss) { }
    List<Vector3Int> upDownDirections = new List<Vector3Int>
    {
        new Vector3Int(0, 1, 0), // Up
        new Vector3Int(0, -1, 0) // Down
    };
    public override IEnumerator Execute()
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
            boss.animator.Play("BossAttack");
            var randomDirection = upDownDirections[Random.Range(0, upDownDirections.Count)];
            var playerCell = GridManager.Instance.WorldToCell(playerObject.transform.position);

            boss.TriggerPencilAttack(1.0f, 20f, randomDirection, playerCell);

            yield return new WaitForSeconds(1.5f);
        }
        yield return new WaitForSeconds(4f);

        isExecuting = false;
        isCompleted = true;
    }
}