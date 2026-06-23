using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BossCommand : ICommand
{
    protected List<Vector3Int> directions = new List<Vector3Int>() {
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0)
    };
    protected readonly BossController boss;
    public bool isExecuting = false;
    public bool isCompleted = false;
    public BossCommand(BossController boss)
    {
        this.boss = boss;
    }

    public virtual async Task Execute()
    {
        // noop
    }

    public virtual void Stop()
    {
        // noop
    }

    public virtual bool IsCompleted()
    {
        return isCompleted;
    }

    public virtual bool IsExecuting()
    {
        return isExecuting;
    }
}