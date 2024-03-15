using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spell_manager : MonoBehaviour
{
    // !!! TODO: IMPORTANT, NEED TO FIX THIS PROBLEM WITH CALLING RTS_CONTROLLER BEFORE EVERY FUNCTION, IT BREAKS DRY!!!
    private RTS_controller rtsController;

    private void Awake()
    {
        GameObject controllerComponent = GameObject.FindWithTag("GameController");
        rtsController = controllerComponent.GetComponent<RTS_controller>();
    }

    public void HelloSpell()
    {
        GameObject controllerComponent = GameObject.FindWithTag("GameController");
        rtsController = controllerComponent.GetComponent<RTS_controller>();
        List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;

        foreach (UnitRTS unit in selectedUnits)
        {
            if (unit is Peasant)
            {
                Debug.Log($"{unit.gameObject.name} says: 'Back to work...'");
            }
        }
    }

    public void FuckYouSpell()
    {
        GameObject controllerComponent = GameObject.FindWithTag("GameController");
        rtsController = controllerComponent.GetComponent<RTS_controller>();

        List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;

        foreach (UnitRTS unit in selectedUnits)
        {
            if (unit is Footman)
            {
                Debug.Log($"{unit.gameObject.name} says: 'Fuck you!'");
            }
        }
    }

    public void ThumbUpSpell()
    {
        GameObject controllerComponent = GameObject.FindWithTag("GameController");
        rtsController = controllerComponent.GetComponent<RTS_controller>();
        List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;

        foreach (UnitRTS unit in selectedUnits)
        {
            if (unit is Footman)
            {
                Debug.Log($"{unit.gameObject.name} says: 'Нє, ну тут чисто лойк, лайкос, так би мовити.'");
            }
        }
    }

    public void SpawnPeasant()
    {
        GameObject controllerComponent = GameObject.FindWithTag("GameController");
        rtsController = controllerComponent.GetComponent<RTS_controller>();
        Debug.Log("I will spawn a peasant");
    }
}
