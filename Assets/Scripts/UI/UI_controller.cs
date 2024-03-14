using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_controller : MonoBehaviour
{
    private static List<GameObject> spellBoxes = new List<GameObject>();

    private void Start()
    {
        GameObject[] spellBoxObjects = GameObject.FindGameObjectsWithTag("SpellBox");
        spellBoxes.AddRange(spellBoxObjects);
    }

    public static void showSpellButtons(List<UnitRTS> selectedUnits)
    {
        // List to keep track of whether a child class has been encountered
        List<System.Type> encounteredTypes = new List<System.Type>();

        foreach (UnitRTS unit in selectedUnits)
        {
            System.Type unitType = unit.GetType();

            if (!encounteredTypes.Contains(unitType))
            {
                encounteredTypes.Add(unitType);
            }

            UnitRTS unitWithHighestPriority = selectedUnits.OrderByDescending(unit => unit.selectionPriority).FirstOrDefault();

            foreach (GameObject spellButton in unitWithHighestPriority.spellButtons)
            {
                SpellButtonInBox(spellButton, spellBoxes);
            }
        }
    }

    public static void showBuildingButtons(RTS_building building)
    {
        foreach (GameObject spellButton in building.spellButtons)
        {
            SpellButtonInBox(spellButton, spellBoxes);
        }
    }

    private static void SpellButtonInBox(GameObject spellButton, List<GameObject> spellBoxes)
    {
        spell_button spellButtonComponent = spellButton.GetComponent<spell_button>();
        int buttonIndex = spellButtonComponent.spellBoxIndex;

        if (spellButtonComponent != null)
        {
            foreach (GameObject spellBox in spellBoxes)
            {
                
                spell_box spellBoxComponent = spellBox.GetComponent<spell_box>();
                if (spellBoxComponent != null && spellBoxComponent.boxIndex == buttonIndex)
                {
                    spellButton.transform.position = spellBox.transform.position;
                    spellButton.transform.localScale = spellBox.transform.localScale;
                    spellButton.SetActive(true);

                    // Exit the loop after finding the matching spell box
                    return;
                }
            }
        }
        else
        {
            Debug.LogError("Spell button component not found on the provided GameObject.");
        }
    }
}
