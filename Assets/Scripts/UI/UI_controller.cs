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
        if (selected == null)
        {
            rtsController.middleSection.enabled = false;
            foreach (var box in progressBoxes)
            {
                box.Setup(rtsController, null, 0);
            }
            UpdateSkills(null);
        }
        else
        {
            var data = selected.OnSelect();
            UpdateSkills(data.Spells);

            if(data.ShowBuildingUI)
            {
                RTS_building building = selected as RTS_building;
                List<UnitRTS> unitsQueue = (building is GoldenMine mine)
                    ? mine.workers.ConvertAll(w => (UnitRTS)w)
                    : building.unitsQueue;
                handleMiddle(unitsQueue);
            }
        }
    }

    public static void UpdateSkills(List<SpellSO> spells)
    {
        if (spells == null || spells.Count == 0)
        {
            foreach (var box in spellBoxes)
            {
                box.Setup(rtsController, null);
            }
        }
        else
        {
            foreach (var box in spellBoxes)
            {
                box.Setup(rtsController, spells.FirstOrDefault(p => p.boxIndex == box.boxIndex));
            }
        }
    }

    public static void handleMiddle(List<UnitRTS> unitsQueue)
    {
        foreach (var box in progressBoxes)
        {
            box.Setup(rtsController, null, 0);
        }

        if (unitsQueue.Count == 0)
        {
            return;
        }

        RTS_building building = rtsController._currentSelected as RTS_building;
        rtsController.middleSection.enabled = true;

        if (rtsController.middleSection != null)
        {
            SpellSO spell = building is GoldenMine mine
                ? mine.freeWorkerSpell
                : building.cancelSpell;

            for (int i = 0; i < unitsQueue.Count; i++)
            {
                if (i < unitsQueue.Count)
                {
                    UnitRTS unit = unitsQueue[i];
                    progressBoxes[i].Setup(rtsController, spell, i);
                }
                else
                {
                    progressBoxes[i].Setup(rtsController, null, i);
                }
            }
        }
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
