using System.Collections.Generic;
using UnityEngine;

public class BossPhase : BossBaseState
{
    List<BossCommand> commands = new List<BossCommand>();
    BossCommand currentCommand;
    public BossPhase(BossController boss, Animator animator, List<BossCommand> initialCommands) : base(boss, animator)
    {
        commands = initialCommands;
    }
    public override void OnEnter()
    {
        currentCommand = commands[Random.Range(0, commands.Count)];
        currentCommand.Execute();
    }

    public override void Update()
    {
        if (currentCommand != null && currentCommand.IsCompleted())
        {
            // Choose a new random command to execute
            currentCommand = commands[Random.Range(0, commands.Count)];
            currentCommand.Execute();
        }
    }

    public override void OnExit()
    {
        // noop
    }
}
