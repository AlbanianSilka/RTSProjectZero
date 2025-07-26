using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using IIndexedSpell = Interfaces.IIndexedSpell;

[CreateAssetMenu(fileName = "CancelTrainingSpell", menuName = "Spells/Cancel Training")]
public class CancelTrainingSpellSO : SpellSO, IIndexedSpell
{
    public int buttonIndex { get; set; }

    public override void Cast(RTS_controller controller)
    {
        // changing player's resources when training canceled
        UnitRTS selectedUnit = controller.selectedBuilding.unitsQueue[buttonIndex];
        controller.owner.ChangePlayerResources(selectedUnit.GetRequiredResources(), "+");

        controller.selectedBuilding.unitsQueue.RemoveAt(buttonIndex);

        if (buttonIndex == 0)
        {
            controller.selectedBuilding.restartSpawnCoroutine();
        }

        UI_controller.handleMiddle(controller.selectedBuilding.unitsQueue);
    }
}
