using System.Collections.Generic;
using UnityEngine;

public class BossPhase : BossBaseState
{
    int health = 3;
    List<BossCommand> attackCommands = new List<BossCommand>();
    BossCommand enterCommand;
    BossCommand exitCommand;
    BossCommand currentCommand;

    float weaponCooldown = 5f;
    float healCooldown = 30f;

    float weaponTimer = 0f;
    float healTimer = 0f;

    public BossPhase(BossController boss, Animator animator, List<BossCommand> attackCommands, int phaseHealth = 3, BossCommand enterCommand = null, BossCommand exitCommand = null) : base(boss, animator)
    {
        this.attackCommands = attackCommands;
        this.health = phaseHealth;
        this.enterCommand = enterCommand;
        this.exitCommand = exitCommand;
    }

    public override void OnEnter()
    {
        Debug.Log("Entering Boss Phase");
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

        weaponTimer += Time.deltaTime;
        if (weaponTimer >= weaponCooldown)
        {
            weaponTimer = 0f;
            boss.TriggerCreateRandomItem(boss.SwordPrefab);
        }

        healTimer += Time.deltaTime;
        if (healTimer >= healCooldown)
        {
            healTimer = 0f;
            boss.TriggerCreateRandomItem(boss.HealPotionPrefab);
        }
    }

    public override void OnExit()
    {
        Sword.OnSwordEffectTriggered -= Hurt;
        exitCommand?.Execute();
        boss.TriggerLowerAllWalls();
    }

    void OnTimerFinished()
    {
        Debug.Log("Done!");
    }

    public void Hurt()
    {
        health = Mathf.Max(0, health - 1);
    }

    public bool IsPhaseEnded()
    {
        return health <= 0;
    }
}
