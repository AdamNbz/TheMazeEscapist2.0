using System.Collections.Generic;
using UnityEngine;

public class BossPhase : BossBaseState
{
    int health = 3;
    int maxHealth = 3;
    List<BossCommand> attackCommands = new List<BossCommand>();
    List<BossCommand> combinedCommands = new List<BossCommand>();
    Vector3Int? endAttackPosition = null;
    BossCommand enterCommand;
    BossCommand exitCommand;
    BossCommand currentCommand;

    Coroutine currentCommandCoroutine;

    float weaponCooldown = 5f;
    float healCooldown = 30f;

    float weaponTimer = 0f;
    float healTimer = 0f;

    public BossPhase(BossController boss, Animator animator, List<BossCommand> attackCommands, int phaseHealth = 3, BossCommand enterCommand = null, BossCommand exitCommand = null, Vector3Int? endAttackPosition = null, List<BossCommand> combinedCommands = null) : base(boss, animator)
    {
        this.attackCommands = attackCommands;
        this.health = phaseHealth;
        this.maxHealth = this.health;
        this.enterCommand = enterCommand;
        this.exitCommand = exitCommand;
        this.endAttackPosition = endAttackPosition;
        this.combinedCommands = combinedCommands ?? new List<BossCommand>();
    }

    public override void OnEnter()
    {
        Debug.Log("Entering Boss Phase");
        currentCommand = enterCommand;
        Sword.OnSwordEffectTriggered += Hurt;
        if (currentCommand != null)
            boss.StartCoroutine(currentCommand.Execute());
    }

    public override void Update()
    {
        if (currentCommand == null || currentCommand.IsCompleted())
        {
            // Choose a new random command to execute
            currentCommand = (health >= (maxHealth / 2) || combinedCommands.Count == 0) ? attackCommands[Random.Range(0, attackCommands.Count)] : combinedCommands[Random.Range(0, combinedCommands.Count)];
            currentCommandCoroutine = boss.StartCoroutine(currentCommand?.Execute());
        }

        weaponTimer += Time.deltaTime;
        if (weaponTimer >= weaponCooldown)
        {
            weaponTimer = 0f;
            if (health > 1 || endAttackPosition == null)
                boss.TriggerCreateRandomItem(boss.SwordPrefab);
            else if (endAttackPosition != null)
            {
                boss.TriggerCreateTile(endAttackPosition.Value, boss.SwordPrefab);
            }
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
        boss.StopCoroutine(currentCommandCoroutine);
        if (exitCommand != null)
            boss.StartCoroutine(exitCommand.Execute());
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
