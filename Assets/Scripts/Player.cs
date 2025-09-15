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
    public TextMeshProUGUI foodText;

    [SerializeField] private MainMenuUI mainMenuUI;
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    protected int currentFoodUsage = 0;
    protected int currentHeroCount = 0;
    protected int maxHeroCount = 1;
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
        resources.Add(Resource.ResourceType.Food, 15);
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

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            mainMenuUI.ToggleMenu();
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
                    
                    // Cap food at maximum of 100
                    if (type == ResourceType.Food)
                    {
                        resources[type] = Mathf.Min(resources[type], 100);
                    }
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
        foodText.text = "Food: " + currentFoodUsage + "/" + resources[Resource.ResourceType.Food];
    }

    public void AddFoodUsage(int foodCost)
    {
        currentFoodUsage += foodCost;
        if (!(this is BotPlayer))
        {
            UpdateResourceUIText();
        }
    }

    public void RemoveFoodUsage(int foodCost)
    {
        currentFoodUsage = Mathf.Max(0, currentFoodUsage - foodCost);
        if (!(this is BotPlayer))
        {
            UpdateResourceUIText();
        }
    }

    public bool CanAffordFood(int foodCost)
    {
        return (currentFoodUsage + foodCost) <= resources[Resource.ResourceType.Food];
    }

    public int GetAvailableFood()
    {
        return resources[Resource.ResourceType.Food] - currentFoodUsage;
    }

    public int GetCurrentFoodUsage()
    {
        return currentFoodUsage;
    }

    public void AddHeroCount()
    {
        currentHeroCount++;
    }

    public void RemoveHeroCount()
    {
        currentHeroCount = Mathf.Max(0, currentHeroCount - 1);
    }

    public bool CanAffordHero()
    {
        return currentHeroCount < maxHeroCount;
    }

    public int GetCurrentHeroCount()
    {
        return currentHeroCount;
    }

    public int GetMaxHeroCount()
    {
        return maxHeroCount;
    }
}
