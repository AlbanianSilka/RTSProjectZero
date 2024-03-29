using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentTree : EnvironmentResource
{
    public override float maxHp => 100f;
    public override float health { get; set; } = 100f;

    private void Awake()
    {
        providedResourceType = Resource.ResourceType.Wood;
    }
}
