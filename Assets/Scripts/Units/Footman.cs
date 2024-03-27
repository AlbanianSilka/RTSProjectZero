using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Footman : UnitRTS
{
    protected override float moveSpeed => 6f;
    protected override float maxHp => 15f;
    protected override float health { get; set; } = 15f;
    protected override float attackDamage => 2f;
    public override float spawnTime => 12f;
    internal override int selectionPriority => 1;

    public Footman()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 25 },
            { ResourceType.Wood, 0 }
        };
    }
}
