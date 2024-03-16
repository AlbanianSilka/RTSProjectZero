using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footman : UnitRTS
{
    protected override float moveSpeed => 6f;
    protected override float maxHp => 15f;
    protected override float attackDamage => 2f;
    public override float spawnTime => 12f;
    internal override int selectionPriority => 1;
}
