using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Footman : UnitRTS
{
    protected override float moveSpeed => 6f;
    protected override float maxHp => 15f;
    protected override float attackDamage => 2f;
    protected override float attackRange => 3.5f;
    public override float spawnTime => 12f;
    internal override int selectionPriority => 2;

    public override float health { get; set; } = 15f;

    public Footman()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 25 },
            { ResourceType.Wood, 0 }
        };
    }

    protected override void Start()
    {
        base.Start();
        attackType = AttackType.Melee;
    }
}
