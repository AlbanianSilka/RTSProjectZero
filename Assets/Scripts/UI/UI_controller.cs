using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Interfaces;

public class UI_controller : MonoBehaviour
{
    public static Canvas rightContainer;
    public static List<spell_box> spellBoxes = new();
    public static Canvas progressBoxContainer;
    public static List<progress_box> progressBoxes = new();
    public static RTS_controller rtsController;

    public static void showSpellButtons(ISelectable selected)
    {
        RTS_controller rts = UI_controller.rtsController;

        if (selected == null)
        {
            UpdateSkills(null);
        }
        else
        {
            var data = selected.OnSelect();
            UpdateSkills(data.Spells);
        }
    }

    public static void UpdateSkills(List<SpellSO> spells)
    {
        RTS_controller rts = UI_controller.rtsController;
        if (spells == null || spells.Count == 0)
        {
            foreach (var box in spellBoxes)
            {
                box.Setup(rts, null);
            }
        }
        else
        {

            foreach (var box in spellBoxes)
            {
                box.Setup(rts, spells.FirstOrDefault(p => p.boxIndex == box.boxIndex));
            }
        }
    }

    // TODO: figure out how to unite with function #2
    public static void handleMiddleSection(List<UnitRTS> unitsQueue, GameObject progressButtonPrefab)
    {
        if(unitsQueue.Count > 0)
        {
            Canvas middleCanvas = rtsController.middleSection;

            // Need to 'restart' canvas so we won't clone buttons each time
            middleCanvas.enabled = false;
            middleCanvas.enabled = true;

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
                        Debug.LogError("You forgot to attach 'progress_button' component to prefab");
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
        Canvas middleCanvas = rtsController.middleSection;

        if (middleCanvas != null)
        {
            middleCanvas.enabled = false;
            middleCanvas.enabled = true;

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
