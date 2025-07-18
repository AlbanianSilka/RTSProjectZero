using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "FuckYouSpell", menuName = "Spells/FuckYou")]
public class FuckYouSpellSO : SpellSO
{
    public override void Cast(RTS_controller controller, List<UnitRTS> units)
    {
        foreach (UnitRTS unit in controller.selectedUnitRTSList)
        {
            if (unit is Footman)
            {
                UnityEngine.Debug.Log($"{unit.gameObject.name} says: 'Fuck you!'");
            }
        }
    }
}
