using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class GoldenMine : RTS_building
{
    public GoldenMine()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 25 },
            { ResourceType.Wood, 25 }
        };
    }
}