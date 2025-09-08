using System.Collections.Generic;
using UnityEngine;

public abstract class SpellSO : ScriptableObject
{
    public string spellName;
    public Sprite icon;

    [Tooltip("UI box this spell button should be placed in.")]
    public int boxIndex = 0;

    [Tooltip("Mana cost to cast this spell. Set to 0 or negative for no mana cost.")]
    public float manaCost = 0f;

    public abstract void Cast(RTS_controller controller);

    protected bool HasEnoughMana(UnitRTS caster)
    {
        if (manaCost <= 0f)
            return true;

        if (caster.mana < manaCost)
        {
            UnityEngine.Debug.Log($"Caster has no mana for {spellName}. Required: {manaCost}, Available: {caster.mana}");
            return false;
        }

        return true;
    }

    protected void DeductMana(UnitRTS caster)
    {
        if (manaCost > 0f)
        {
            caster.mana -= manaCost;
            UnityEngine.Debug.Log($"Spell {spellName} mana deducted. Mana remaining: {caster.mana}");
        }
    }
}
