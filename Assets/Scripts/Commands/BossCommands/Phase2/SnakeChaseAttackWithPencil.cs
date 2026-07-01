using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class SnakeChaseAttackWithPencil : BossCommand
{
    public SnakeChaseAttackWithPencil(BossController boss) : base(boss) { }
    Vector3Int currentCell;
    Vector3Int currentDirection = Vector3Int.zero;
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        var snakeAttack = new SnakeChaseAttack(boss);
        var pencilAttack = new RandomFourDirectionAttack(boss);

        pencilAttack.Execute();
        yield return snakeAttack.Execute();

        isExecuting = false;
        isCompleted = true;
    }
}

