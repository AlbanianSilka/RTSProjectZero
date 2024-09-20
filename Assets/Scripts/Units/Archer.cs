using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Archer : UnitRTS
{
    protected override float moveSpeed => 6f;
    protected override float maxHp => 10f;
    protected override float attackDamage => 1f;
    protected override float attackRange => 20f;
    public override float spawnTime => 9f;
    internal override int selectionPriority => 1;

    public override float health { get; set; } = 10f;

    public Archer()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 20 },
            { ResourceType.Wood, 0 }
        };
    }

    protected override void Start()
    {
        base.Start();
        attackType = AttackType.Ranged;
    }
}
