using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeByThreeWithPencilAttack : BossCommand
{
    public ThreeByThreeWithPencilAttack(BossController boss) : base(boss) { }
    private List<Vector3Int> allDirections = new List<Vector3Int>() {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(0, 0, 0)
    };
    public override IEnumerator Execute()
    {
        isExecuting = true;
        isCompleted = false;

        var threeAttack = new ThreeByThreeAttack(boss);
        var pencilAttack = new RandomFourDirectionAttack(boss);

        bool threeDone = false;
        bool pencilDone = false;

        var threeCoroutine = boss.StartCoroutine(RunAndFlag(threeAttack.Execute(), () => threeDone = true));
        var pencilCoroutine = boss.StartCoroutine(RunAndFlag(pencilAttack.Execute(), () => pencilDone = true));

        activeCoroutines.Add(threeCoroutine);
        activeCoroutines.Add(pencilCoroutine);

        yield return new WaitUntil(() => threeDone && pencilDone);

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
