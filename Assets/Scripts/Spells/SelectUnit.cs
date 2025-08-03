using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using IIndexedSpell = Interfaces.IIndexedSpell;

[CreateAssetMenu(fileName = "SelectUnitSpell", menuName = "Spells/SelectUnit")]
public class SelectUnit : SpellSO, IIndexedSpell
{
    public int buttonIndex { get; set; }

    public override void Cast(RTS_controller controller)
    {
        UnitRTS selectedUnit = controller.selectedUnitRTSList[buttonIndex];
        icon = selectedUnit.unitIcon;

        foreach (var unit in controller.selectedUnitRTSList)
        {
            if (unit != selectedUnit)
            {
                unit.SetSelected(false);
            }
        }

        controller.selectedUnitRTSList.Clear();
        controller.selectedUnitRTSList.Add(selectedUnit);
        UI_controller.handleMiddle(controller.selectedUnitRTSList);
    }
}
