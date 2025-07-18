using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "ThumbUpSpell", menuName = "Spells/ThumbUp")]
public class ThumbUpSpell : SpellSO
{
    public override void Cast(RTS_controller controller, List<UnitRTS> units)
    {
        foreach (UnitRTS unit in controller.selectedUnitRTSList)
        {
            if (unit is Footman)
            {
                UnityEngine.Debug.Log($"{unit.gameObject.name} says: 'Нє, ну тут чисто лойк, лайкос, так би мовити.'");
            }
        }
    }
}
