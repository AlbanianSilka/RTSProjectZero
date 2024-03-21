using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spell_button : MonoBehaviour
{
    public int spellBoxIndex;

    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
