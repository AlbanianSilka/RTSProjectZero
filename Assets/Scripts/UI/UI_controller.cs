using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Interfaces;
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

    public static void showSpellButtons()
    {
        // RTS_controller rts = UI_controller.rtsController;
        //
        //
        //
        //
        // // CASE 1: If a building is selected
        // if (rts.selectedBuilding != null)
        // {
        //     RTS_building building = rts.selectedBuilding;
        //
        //     foreach (SpellSO spell in building.assignedSpells)
        //     {
        //         CreateSpellButton(spell, building);
        //     }
        //
        //     return;
        // }
        //
        // // CASE 2: If units are selected
        // List<System.Type> encounteredTypes = new();
        // List<UnitRTS> selectedUnits = rts.selectedUnitRTSList;
        //
        // foreach (UnitRTS unit in selectedUnits)
        // {
        //     Type unitType = unit.GetType();
        //     if (!encounteredTypes.Contains(unitType))
        //     {
        //         encounteredTypes.Add(unitType);
        //     }
        //     else continue;
        //
        //     UnitRTS unitWithHighestPriority = selectedUnits
        //         .Where(u => u.GetType() == unitType)
        //         .OrderByDescending(u => u.selectionPriority)
        //         .FirstOrDefault();
        //
        //     foreach (SpellSO spell in unitWithHighestPriority.assignedSpells)
        //     {
        //         CreateSpellButton(spell, selectedUnits);
        //     }
        // }
    }
    
    public static void ShowSpellButtons(ISelectable selected)
    {
        RTS_controller rts = UI_controller.rtsController;

        if (selected == null)
        {
            foreach (var box in spellBoxes)
            {
                box.Setup(rtsController, null);
            }
        }
        else
        {
            var data = selected.OnSelect();
            foreach (var box in spellBoxes)
            {
                box.Setup(rtsController, data.Spells.FirstOrDefault(p => p.boxIndex == box.boxIndex));
            }
        }
    }

    // public static void CreateSpellButton(SpellSO spell, object context)
    // {
    //     var prefab = UI_controller.rtsController.spellButtonPrefab;
    //
    //     if (prefab == null)
    //     {
    //         Debug.LogError("spellButtonPrefab is not assigned in RTS_controller.");
    //         return;
    //     }
    //
    //     spell_button newSpellButton = GameObject.Instantiate(prefab);
    //     newSpellButton.assignedSpell = spell;
    //     newSpellButton.spellBoxIndex = spell.boxIndex;
    //
    //     Button uiButton = newSpellButton.GetComponent<Button>();
    //     if (uiButton != null)
    //     {
    //         if (context is List<UnitRTS> units)
    //         {
    //             uiButton.onClick.AddListener(() => spell.Cast(UI_controller.rtsController));
    //         }
    //         else if (context is RTS_building building)
    //         {
    //             uiButton.onClick.AddListener(() => spell.Cast(UI_controller.rtsController));
    //         }
    //         else
    //         {
    //             Debug.LogError("Unsupported spell context passed to CreateSpellButton.");
    //         }
    //     }
    //
    //     Image img = newSpellButton.GetComponent<Image>();
    //     if (img != null && spell.icon != null)
    //     {
    //         img.sprite = spell.icon;
    //     }
    //     else
    //     {
    //         Debug.LogWarning("Spell button is missing an Image component or spell has no icon.");
    //     }
    //
    //     SpellButtonInBox(newSpellButton.gameObject);
    // }

    // public static void showPeasantBuildingButtons(Peasant selectedUnit)
    // {
    //     var unitList = new List<UnitRTS> { selectedUnit };
    //
    //     foreach (SpellSO buildSpell in selectedUnit.buildingButtons)
    //     {
    //         CreateSpellButton(buildSpell, unitList);
    //     }
    // }

    // private static void SpellButtonInBox(GameObject spellButton)
    // {
    //     if(rightContainer != null)
    //     {
    //         spellButton.transform.SetParent(rightContainer.transform, false);
    //
    //         spell_button spellButtonComponent = spellButton.GetComponent<spell_button>();
    //         int buttonIndex = spellButtonComponent.spellBoxIndex;
    //
    //         if (spellButtonComponent != null)
    //         {
    //             foreach (spell_box spellBox in spellBoxes)
    //             {
    //
    //                 spell_box spellBoxComponent = spellBox.GetComponent<spell_box>();
    //                 if (spellBoxComponent != null && spellBoxComponent.boxIndex == buttonIndex)
    //                 {
    //                     spellButton.transform.position = spellBox.transform.position;
    //                     spellButton.transform.localScale = spellBox.transform.localScale;
    //                     spellButton.SetActive(true);
    //
    //                     // Exit the loop after finding the matching spell box
    //                     return;
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogError("Spell button component not found on the provided GameObject.");
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("Canvas for spells not found, don't forget to add it");
    //     }
    // }

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
