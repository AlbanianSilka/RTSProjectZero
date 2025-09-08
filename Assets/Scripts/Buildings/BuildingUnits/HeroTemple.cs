using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class HeroTemple : RTS_building {
    public override float health { get; set; } = 10f;

    public HeroTemple() {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 30 },
            { ResourceType.Wood, 30 }
        };
    }
}