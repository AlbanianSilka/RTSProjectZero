using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spell_box : MonoBehaviour
{
    private static HashSet<int> usedIndices = new HashSet<int>(); // Collection to store used indices

    public int index; // Index of the spell box

    private void Start()
    {
        AssignUniqueIndex();
    }

    private void AssignUniqueIndex()
    {
        int newIndex = 0; // Start indexing from 0

        // Find the next available index
        while (usedIndices.Contains(newIndex))
        {
            newIndex++;
        }

        index = newIndex;

        // Add the index to the collection of used indices
        usedIndices.Add(newIndex);
    }
}
