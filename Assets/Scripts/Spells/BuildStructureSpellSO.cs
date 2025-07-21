using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildStructure", menuName = "Spells/Build Structure")]
public class BuildStructureSpellSO : SpellSO
{
    public GameObject buildingPrefab;

    public override void Cast(RTS_controller controller)
    {
        if (buildingPrefab == null)
        {
            UI_controller.showSpellButtons(controller._currentSelected);
            return;
        }

        controller.BuildingManager.buildingPrefab = buildingPrefab;
        controller.BuildingManager.startBuilding();
    }
}
