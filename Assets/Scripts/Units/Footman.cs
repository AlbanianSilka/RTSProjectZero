using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footman : UnitRTS
{
    protected override float moveSpeed => 6f;
    internal override int selectionPriority => 1;

    private void Awake()
    {
        foreach (GameObject spellButton in spellButtons)
        {
            Debug.Log("Spell button name is: " + spellButton.name);
        }
    }
}
