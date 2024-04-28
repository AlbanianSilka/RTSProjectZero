using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Castle : RTS_building
{
    public override float health { get; set; } = 35f;

    public Castle()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 50 },
            { ResourceType.Wood, 50 }
        };
    }
}
