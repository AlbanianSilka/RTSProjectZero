using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Because this resource will be recovered via mines - it will have another class
// Currently I have only golden deposit, so I won't create a child class
// Maybe later if there will be more mine types
public class Deposit : MonoBehaviour
{
    public Resource.ResourceType providedResourceType;
    // default number for gold amount would be a 1000 but maybe I could've make a randomizer
    public int resourceAmount = 1000;

    private void Awake()
    {
        providedResourceType = Resource.ResourceType.Gold;
    }
}
