using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Tower : RTS_building
{
    public override float health { get; set; } = 15f;

    protected override float attackSpeed => 1f;
    protected override float attackRange => 15f;
    protected override float attackDamage => 1f;

    public Tower()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 20 },
            { ResourceType.Wood, 20 }
        };
    }
}


