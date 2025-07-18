using System.Collections.Generic;
using UnityEngine;

public abstract class SpellSO : ScriptableObject
{
    public string spellId;
    public string spellName;
    public Sprite icon;

    [Tooltip("UI box this spell button should be placed in.")]
    public int boxIndex = 0;

    public virtual void Cast(RTS_controller controller, List<UnitRTS> units) { }
    public virtual void Cast(RTS_controller controller, RTS_building building) { }
}
