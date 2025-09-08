using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Interfaces;
using Data;
using static Resource;

public class Archmage : Hero {
    protected override float attackDamage => 5f;
    protected override float attackRange => 5f;
    protected override float arrowSpeed => 15f;
    protected override float maxMana { get; set; } = 100f;

    public override float mana { get; set; } = 100f;
    public override float maxHealth { get; set; } = 100f;
    public override float health { get; set; } = 100f;

    internal override int selectionPriority => 2;

    public Archmage() {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 50 },
            { ResourceType.Wood, 0 }
        };
    }

    protected override void Start()
    {
        base.Start();
        attackType = AttackType.Ranged;
    }
}