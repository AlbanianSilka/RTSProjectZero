using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildStructure", menuName = "Spells/Build Structure")]
public class BuildStructureSpellSO : SpellSO
{
    public GameObject buildingPrefab;

    public override void Cast(RTS_controller controller, List<UnitRTS> units)
    {
        controller.BuildingManager.buildingPrefab = buildingPrefab;
        controller.BuildingManager.startBuilding();
    }
}
