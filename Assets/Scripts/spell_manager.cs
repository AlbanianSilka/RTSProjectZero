using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spell_manager : MonoBehaviour
{
    private RTS_controller rtsController;

    private void InitializeController()
    {
        GameObject controllerComponent = GameObject.FindWithTag("GameController");
        rtsController = controllerComponent.GetComponent<RTS_controller>();
    }

    public void HelloSpell()
    {
        InitializeController();

        foreach (UnitRTS unit in rtsController.selectedUnitRTSList)
        {
            if (unit is Peasant)
            {
                Debug.Log($"{unit.gameObject.name} says: 'Back to work...'");
            }
        }
    }

    public void FuckYouSpell()
    {
        InitializeController();

        foreach (UnitRTS unit in rtsController.selectedUnitRTSList)
        {
            if (unit is Footman)
            {
                Debug.Log($"{unit.gameObject.name} says: 'Fuck you!'");
            }
        }
    }

    public void ThumbUpSpell()
    {
        InitializeController();

        foreach (UnitRTS unit in rtsController.selectedUnitRTSList)
        {
            if (unit is Footman)
            {
                Debug.Log($"{unit.gameObject.name} says: 'Нє, ну тут чисто лойк, лайкос, так би мовити.'");
            }
        }
    }

    public void SpawnUnit(UnitRTS unit)
    {
        InitializeController();

        if (rtsController.selectedBuilding != null)
        {
            rtsController.selectedBuilding.AddUnitToQueue(unit);
        }
        else
        {
            Debug.LogError("No building selected but SpawnUnit spell called.");
        }
    }
}
