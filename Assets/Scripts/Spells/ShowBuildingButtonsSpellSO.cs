using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "ShowBuildingButtons", menuName = "Spells/Show Building Buttons")]
public class ShowBuildingButtonsSpellSO : SpellSO
{
    public override void Cast(RTS_controller controller, List<UnitRTS> units)
    {
        UnitRTS selectedUnit = controller.selectedUnitRTSList[0];
        Peasant selectedPeasant = selectedUnit.GetComponent<Peasant>();
        UI_controller.showPeasantBuildingButtons(selectedPeasant);
    }
}
