using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnUnitSpell", menuName = "Spells/Spawn Unit")]
public class SpawnUnitSpellSO : SpellSO
{
    public UnitRTS unitToTrain;

    public override void Cast(RTS_controller controller)
    {
        RTS_building building = controller.selectedBuilding;

        if (unitToTrain == null || building == null)
        {
            UnityEngine.Debug.LogError("Unit prefab or building is null in SpawnUnitSpell.");
            return;
        }

        // Check if trying to train a hero and if hero limit is reached
        if (unitToTrain is Hero && !controller.owner.CanAffordHero())
        {
            UnityEngine.Debug.Log("Cannot train " + unitToTrain.name + ". Hero limit reached (" + controller.owner.GetCurrentHeroCount() + "/" + controller.owner.GetMaxHeroCount() + ").");
            return;
        }

        if (unitToTrain.CanBeTrained(controller.owner))
        {
            building.AddUnitToQueue(unitToTrain);
        }
        else
        {
            if (!controller.owner.CanAffordFood(unitToTrain.foodCost))
            {
                UnityEngine.Debug.Log("Not enough food to train " + unitToTrain.name + ". Need " + unitToTrain.foodCost + " food, have " + controller.owner.GetAvailableFood() + " available.");
            }
            else
            {
                UnityEngine.Debug.Log("Not enough resources to train " + unitToTrain.name);
            }
        }
    }
}
