using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_controller : MonoBehaviour
{
    public static Canvas rightContainer;
    public static List<spell_box> spellBoxes = new();
    public static Canvas progressBoxContainer;
    public static List<progress_box> progressBoxes = new();
    public static RTS_controller rtsController;

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
            } else
            {
                continue;
            }

            UnitRTS unitWithHighestPriority = selectedUnits.OrderByDescending(unit => unit.selectionPriority).FirstOrDefault();

            foreach (GameObject spellButton in unitWithHighestPriority.spellButtons)
            {
                SpellButtonInBox(spellButton);
            }
        }
    }

    public static void showPeasantBuildingButtons(Peasant selectedUnit)
    {
        foreach (GameObject spellButton in selectedUnit.buildingButtons)
        {
            SpellButtonInBox(spellButton);
        }
    }

    public static void showBuildingButtons(RTS_building building)
    {
        foreach (GameObject spellButton in building.spellButtons)
        {
            SpellButtonInBox(spellButton);
        }
    }

    private static void SpellButtonInBox(GameObject spellButton)
    {
        GameObject newSpellButton = Instantiate(spellButton); 

        if(rightContainer != null)
        {
            newSpellButton.transform.SetParent(rightContainer.transform, false);

            spell_button spellButtonComponent = newSpellButton.GetComponent<spell_button>();
            int buttonIndex = spellButtonComponent.spellBoxIndex;

            if (spellButtonComponent != null)
            {
                foreach (spell_box spellBox in spellBoxes)
                {

                    spell_box spellBoxComponent = spellBox.GetComponent<spell_box>();
                    if (spellBoxComponent != null && spellBoxComponent.boxIndex == buttonIndex)
                    {
                        newSpellButton.transform.position = spellBox.transform.position;
                        newSpellButton.transform.localScale = spellBox.transform.localScale;
                        newSpellButton.SetActive(true);

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
        else
        {
            Debug.LogError("Canvas for spells not found, don't forget to add it");
        }
    }

    // TODO: figure out how to unite with function #2
    public static void handleMiddleSection(List<UnitRTS> unitsQueue, GameObject progressButtonPrefab)
    {
        if(unitsQueue.Count > 0)
        {
            GameObject middleCanvas = GameObject.FindGameObjectWithTag("MiddleSection");

            // Need to 'restart' canvas so we won't clone buttons each time
            middleCanvas.SetActive(false);
            middleCanvas.SetActive(true);

            if (middleCanvas != null)
            {
                GameObject progressButton;
                progress_button buttonComponent;

                for( int index = 0; index < unitsQueue.Count; index++)
                {
                    UnitRTS unit = unitsQueue[index];
                    progressButton = Instantiate(progressButtonPrefab);
                    buttonComponent = progressButton.GetComponent<progress_button>();
                    buttonComponent.rtsController = rtsController;
                    progressButton.transform.SetParent(middleCanvas.transform, false);

                    if (buttonComponent != null)
                    {
                        buttonComponent.buttonIndex = index;
                        buttonComponent.maxTrainingTime = unit.spawnTime;
                        changeProgressIcon(unit, index, progressButton);
                    }
                    else
                    {
                        Debug.LogError("You forgot to attach 'spell_button' component to prefab");
                    }
                }
            }
            else
            {
                Debug.LogError("You forgot to add middle canvas to the scene");
            };
        }
    }

    // TODO: figure out how to unite with function #1
    public static void handleMineMiddle(List<Peasant> unitsQueue, GameObject workerButtonPrefab)
    {
        GameObject middleCanvas = GameObject.FindGameObjectWithTag("MiddleSection");

        if (middleCanvas != null)
        {
            middleCanvas.SetActive(false);
            middleCanvas.SetActive(true);

            GameObject workerButton;
            worker_button workerComponent;

            for (int index = 0; index < unitsQueue.Count; index++)
            {
                UnitRTS unit = unitsQueue[index];
                workerButton = Instantiate(workerButtonPrefab);
                workerButton.transform.SetParent(middleCanvas.transform, false);

                if (workerButton.GetComponent<worker_button>() != null)
                {
                    workerComponent = workerButton.GetComponent<worker_button>();
                    workerComponent.rtsController = rtsController;
                    workerComponent.buttonIndex = index;
                    changeProgressIcon(unit, index, workerButton);
                }
                else
                {
                    Debug.LogError("You forgot to attach 'worker_button' component to prefab");
                }
            }
        }
        else
        {
            Debug.LogError("You forgot to add middle canvas to the scene");
        };
    }

    private static void changeProgressIcon(UnitRTS unit, int buttonIndex, GameObject progressButton)
    {
        Sprite unitIcon = unit.unitIcon;
        Image newBtnImg = progressButton.GetComponent<Image>();
        newBtnImg.sprite = unitIcon;
        foreach (progress_box progressBox in progressBoxes)
        {
            progress_box progressBoxComponent = progressBox.GetComponent<progress_box>();
            if (progressBoxComponent != null && progressBoxComponent.boxIndex == buttonIndex)
            {
                progressButton.transform.position = progressBox.transform.position;
                progressButton.transform.localScale = progressBox.transform.localScale;
                progressButton.SetActive(true);

                // Exit the loop after finding the matching progress box
                return;
            }
        }
    }
}
