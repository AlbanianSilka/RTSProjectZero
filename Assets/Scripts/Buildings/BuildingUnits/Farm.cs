using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Farm : RTS_building
{
    public override float health { get; set; } = 10;
    private const int FOOD_PROVIDED = 15;
    private bool hasAddedFood = false;

    public Farm()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 25 },
            { ResourceType.Wood, 25 }
        };
    }

    protected override void Start()
    {
        base.Start();
        
        // Check if building is already finished when Start() is called
        if (finished && owner != null)
        {
            owner.ChangePlayerResources(new Dictionary<ResourceType, int> { { ResourceType.Food, FOOD_PROVIDED } }, "+");
        }
    }

    protected void Update()
    {
        if (finished && owner != null)
        {
            // Only add food once when building becomes finished
            if (!hasAddedFood)
            {
                owner.ChangePlayerResources(new Dictionary<ResourceType, int> { { ResourceType.Food, FOOD_PROVIDED } }, "+");
                hasAddedFood = true;
            }
        }
    }

    protected override void Die()
    {
        // Remove food capacity when farm is destroyed (only if we had added it)
        if (owner != null && hasAddedFood)
        {
            owner.ChangePlayerResources(new Dictionary<ResourceType, int> { { ResourceType.Food, FOOD_PROVIDED } }, "-");
        }
        
        base.Die();
    }
}
