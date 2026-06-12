using System.Collections.Generic;
using UnityEngine;

public class BossPhase : BossBaseState
{
    int health = 3;
    List<BossCommand> attackCommands = new List<BossCommand>();
    BossCommand enterCommand;
    BossCommand exitCommand;
    BossCommand currentCommand;

    public BossPhase(BossController boss, Animator animator, List<BossCommand> attackCommands, int phaseHealth = 3, BossCommand enterCommand = null, BossCommand exitCommand = null) : base(boss, animator)
    {
        this.attackCommands = attackCommands;
        this.health = phaseHealth;
        this.enterCommand = enterCommand;
        this.exitCommand = exitCommand;
    }

    public override void OnEnter()
    {
        currentCommand = enterCommand;
        Sword.OnSwordEffectTriggered += Hurt;
        currentCommand?.Execute();
    }

    public override void Update()
    {
        if (currentCommand == null || currentCommand.IsCompleted())
        {
            // Choose a new random command to execute
            currentCommand = attackCommands[Random.Range(0, attackCommands.Count)];
            currentCommand.Execute();
        }
    }

    public override void OnExit()
    {
        Sword.OnSwordEffectTriggered -= Hurt;
        exitCommand?.Execute();
        boss.TriggerLowerAllWalls();
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

        while (!GridManager.Instance.IsWalkable(randomCell))
        {
            randomCell = new Vector3Int(Random.Range((int)boss.originCell.x, (int)boss.originCell.x + boss.size), Random.Range((int)boss.originCell.y - boss.size, (int)boss.originCell.y), 0);
        }
        boss.TriggerCreateTile(randomCell, boss.SwordPrefab);
    }

    public bool IsPhaseEnded()
    {
        return health <= 0;
    }
}
