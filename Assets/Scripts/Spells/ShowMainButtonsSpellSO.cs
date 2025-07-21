using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "ShowMainButtons", menuName = "Spells/Show Main Buttons")]
public class ShowMainButtonsSpellSO : SpellSO
{
    public override void Cast(RTS_controller controller)
    {
        UI_controller.showSpellButtons(controller._currentSelected);
    }
}

