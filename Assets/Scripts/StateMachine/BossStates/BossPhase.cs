using System.Collections.Generic;
using UnityEngine;

public class BossPhase : BossBaseState
{
    int health = 3;
    List<BossCommand> commands = new List<BossCommand>();
    BossCommand currentCommand;
    public BossPhase(BossController boss, Animator animator, List<BossCommand> initialCommands) : base(boss, animator)
    {
        commands = initialCommands;
        health = 3;
    }

    public BossPhase(BossController boss, Animator animator, List<BossCommand> initialCommands, int phaseHealth) : base(boss, animator)
    {
        commands = initialCommands;
        health = phaseHealth;
    }

    public override void OnEnter()
    {
        Debug.Log("Entering Boss Phase");
        currentCommand = commands[Random.Range(0, commands.Count)];
        currentCommand.Execute();
        Sword.OnSwordEffectTriggered += Hurt;
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
        Sword.OnSwordEffectTriggered -= Hurt;
        Debug.Log("Exiting Boss Phase");
    }

    public void Hurt()
    {
        health--;
        if (health <= 0)
        {
            Debug.Log("Boss phase defeated! Transitioning to next phase...");
        }
        else
        {
            Debug.Log("Boss hurt! Remaining health: " + health);
        }
        // Spawn a random sword
        var randomCell = new Vector3Int(Random.Range((int)boss.originCell.x, (int)boss.originCell.x + boss.size), Random.Range((int)boss.originCell.y - boss.size, (int)boss.originCell.y), 0);
        boss.TriggerCreateTile(randomCell, boss.SwordPrefab);
    }

    public bool IsPhaseEnded()
    {
        return health <= 0;
    }
}
