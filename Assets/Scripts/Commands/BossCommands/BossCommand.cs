using System.Threading.Tasks;

public class BossCommand : ICommand
{
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