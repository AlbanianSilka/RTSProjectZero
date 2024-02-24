using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footman : UnitRTS
{
    protected override float moveSpeed => 6f;
    internal override int selectionPriority => 1;
}
