using Interfaces;
using UnityEngine;

public static class SpellFactory
{
    public static SpellSO CreateClonedSpell(SpellSO original, int index)
    {
        if (original is IIndexedSpell)
        {
            SpellSO clone = ScriptableObject.Instantiate(original); // shallow copy
            if (clone is IIndexedSpell indexed)
            {
                indexed.buttonIndex = index;
            }
            return clone;
        }

        return original;
    }
}
