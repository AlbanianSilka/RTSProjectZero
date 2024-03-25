using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Player : MonoBehaviour
{
    [SerializeField] public string team;

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    // Method to add resources to the player
    public void AddResource(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
        }
        else
        {
            resources.Add(type, amount);
        }
    }

    public bool HasEnoughResources(ResourceType type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            return resources[type] >= amount;
        }
        return false;
    }
}
