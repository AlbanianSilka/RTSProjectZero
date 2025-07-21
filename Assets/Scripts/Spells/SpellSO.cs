using System.Collections.Generic;
using UnityEngine;

public abstract class SpellSO : ScriptableObject
{
    public string spellName;
    public Sprite icon;

    [Tooltip("UI box this spell button should be placed in.")]
    public int boxIndex = 0;

    public abstract void Cast(RTS_controller controller);
}
