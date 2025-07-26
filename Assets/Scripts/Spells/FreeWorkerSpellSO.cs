using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using IIndexedSpell = Interfaces.IIndexedSpell;

[CreateAssetMenu(fileName = "FreeWorkerSpell", menuName = "Spells/Free Worker")]
public class FreeWorkerSpellSO : SpellSO, IIndexedSpell
{
    public int buttonIndex { get; set; }

    public override void Cast(RTS_controller controller)
    {
        GoldenMine mine = controller.selectedBuilding as GoldenMine;
        mine.removeWorkerByIndex(buttonIndex);

        List<UnitRTS> units = mine.workers.ConvertAll(w => (UnitRTS)w);
        UI_controller.handleMiddle(units);
    }
}
