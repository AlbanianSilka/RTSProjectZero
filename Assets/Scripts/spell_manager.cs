using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spell_manager : MonoBehaviour
{
    private RTS_controller rtsController;

    private void Awake()
    {
        rtsController = FindObjectOfType<RTS_controller>();
    }

    public void HelloSpell()
    {
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
        Debug.Log("I will spawn a peasant");
    }
}
