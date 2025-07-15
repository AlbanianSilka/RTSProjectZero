using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Resource;

public class Player : MonoBehaviour
{
    [SerializeField] public string team;
    [SerializeField] public Color teamColor = Color.white;
    public RTS_controller rtsController;
    // placed inside canvas
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI woodText;

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    private static List<Color> availableColors = new()
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.magenta,
        Color.cyan
    };

    // starting resources for players
    public Player()
    {
        resources.Add(Resource.ResourceType.Gold, 150); 
        resources.Add(Resource.ResourceType.Wood, 150); 
    }

    protected virtual void Awake()
    {
        RTS_controller newController = Instantiate(rtsController, transform.position, Quaternion.identity);
        rtsController = newController.GetComponent<RTS_controller>();
        newController.owner = this;
        UpdateResourceUIText();
        if (teamColor == Color.white)
        {
            int index = Random.Range(0, availableColors.Count);
            teamColor = availableColors[index];
        }
    }

    public void ChangePlayerResources(Dictionary<ResourceType, int> resourceChanges, string sign)
    {
        foreach (var kvp in resourceChanges)
        {
            ResourceType type = kvp.Key;
            int amount = kvp.Value;

            if (resources.ContainsKey(type))
            {
                if(sign == "-")
                {
                    resources[type] -= amount;
                } else if (sign == "+")
                {
                    resources[type] += amount;
                } else
                {
                    Debug.LogError("Wrong sign used");
                }

                // Only update UI for non-bot players
                if (!(this is BotPlayer))
                {
                    UpdateResourceUIText();
                }
            }
            else
            {
                Debug.LogError($"There is no such resource as {type}");
            }
        }

        // I'll delete it a bit later, now it just for detecting changes in player resources
        foreach (var kvp in resources)
        {
            Debug.Log("Resource: " + kvp.Key + ", Amount: " + kvp.Value);
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

    private void UpdateResourceUIText()
    {
        goldText.text = "Gold: " + resources[Resource.ResourceType.Gold];
        woodText.text = "Wood: " + resources[Resource.ResourceType.Wood];
    }
}
