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
        var pencilAttack = new RandomTwoDirectionAttack(boss);

        bool snakeDone = false;
        bool pencilDone = false;

        var snakeCoroutine = boss.StartCoroutine(RunAndFlag(snakeAttack.Execute(), () => snakeDone = true));
        var pencilCoroutine = boss.StartCoroutine(RunAndFlag(pencilAttack.Execute(), () => pencilDone = true));

        activeCoroutines.Add(snakeCoroutine);
        activeCoroutines.Add(pencilCoroutine);

        yield return new WaitUntil(() => snakeDone && pencilDone);

        isExecuting = false;
        isCompleted = true;
    }

    IEnumerator RunAndFlag(IEnumerator routine, System.Action onDone)
    {
        yield return routine;
        onDone();
    }

    public override void StopExecution()
    {
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
            {
                boss.StopCoroutine(coroutine);
            }
        }
        activeCoroutines.Clear();
        isExecuting = false;
        isCompleted = true;
    }
}

