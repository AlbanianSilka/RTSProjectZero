using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "HelloSpell", menuName = "Spells/Hello")]
public class HelloSpellSO : SpellSO
{
    public override void Cast(RTS_controller controller)
    {
        foreach (UnitRTS unit in controller.selectedUnitRTSList)
        {
            if (unit is Peasant)
            {
                UnityEngine.Debug.Log($"{unit.gameObject.name} says: 'Back to work...'");
            }
        }
    }
}
