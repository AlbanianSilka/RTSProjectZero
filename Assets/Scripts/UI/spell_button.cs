using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class spell_button : MonoBehaviour
{
    public SpellSO assignedSpell;
    public int spellBoxIndex;

    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
