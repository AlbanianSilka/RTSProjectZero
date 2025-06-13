using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class BotPlayer : Player
{
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    private List<UnitRTS> friendlyUnits = new List<UnitRTS>();
    private List<RTS_building> friendlyBuildings = new List<RTS_building>();

    private enum BotTask
    {
        CheckForCastle,
        BuildCastle
    }

    private Queue<BotTask> taskQueue = new Queue<BotTask>();

    public BotPlayer()
    {
        resources.Add(Resource.ResourceType.Gold, 150);
        resources.Add(Resource.ResourceType.Wood, 150);
    }

    protected override void Awake()
    {
        RTS_controller newController = Instantiate(rtsController, transform.position, Quaternion.identity);
        rtsController = newController.GetComponent<RTS_controller>();
        rtsController.owner = this;
    }

    void Start()
    {
        FindFriendlyUnits();
        FindFriendlyBuildings();
        Peasant builder = GetAvailableBuilder();

        if (builder != null)
        {
            Debug.Log("[BotPlayer] Found a builder: " + builder.name);
        }
        else
        {
            Debug.LogWarning("[BotPlayer] No builder (Peasant) found for team: " + team);
        }

        taskQueue.Enqueue(BotTask.CheckForCastle);

        StartCoroutine(RunAITasks());
    }

    private void FindFriendlyUnits()
    {
        UnitRTS[] allUnits = GameObject.FindObjectsOfType<UnitRTS>();

        foreach (UnitRTS unit in allUnits)
        {
            if (unit.team == team)
            {
                friendlyUnits.Add(unit);
                unit.owner = this;
            }
        }
    }

    private void FindFriendlyBuildings()
    {
        RTS_building[] allBuildings = GameObject.FindObjectsOfType<RTS_building>();

        foreach (RTS_building building in allBuildings)
        {
            if (building.team == team)
            {
                friendlyBuildings.Add(building);
                building.owner = this;
            }
        }
    }

    private Peasant GetAvailableBuilder()
    {
        foreach (UnitRTS unit in friendlyUnits)
        {
            if (unit is Peasant peasant)
            {
                return peasant;
            }
        }
        return null;
    }

    // ######### Bot tasks part #########
    private IEnumerator RunAITasks()
    {
        while (true)
        {
            if (taskQueue.Count > 0)
            {
                BotTask task = taskQueue.Dequeue();
                ExecuteTask(task);
            }

            yield return new WaitForSeconds(1f); // Delay between tasks
        }
    }

    private void ExecuteTask(BotTask task)
    {
        switch (task)
        {
            case BotTask.CheckForCastle:
                bool hasCastle = HasCastle();
                if (!hasCastle)
                {
                    Debug.Log("[BotPlayer] No Castle present — should build one.");
                    taskQueue.Enqueue(BotTask.BuildCastle);
                }
                else
                {
                    Debug.Log("[BotPlayer] Castle is present.");
                }
                break;

            case BotTask.BuildCastle:
                BuildCastleTask();
                break;

        // TODO: probably would need to move this part to a seperate class, might be too many cases
        }
    }

    private bool HasCastle()
    {
        foreach (var building in friendlyBuildings)
        {
            if (building is Castle) return true;
        }

        return false;
    }

    private void BuildCastleTask()
    {
        Peasant builder = GetAvailableBuilder();

        if (builder == null)
        {
            Debug.LogWarning("[BotPlayer] No available builder to construct the Castle.");
            return;
        }
        List<Peasant> singleBuilderList = new List<Peasant> { builder };

        // Would search for castle prefab in our dictionary
        GameObject castlePrefab = BuildingRegistry.Instance.GetBuildingPrefab("castle");
        rtsController.BuildingManager.buildingPrefab = castlePrefab;

        // Would first decide a position of a builder and based on that bot would place his first building
        Vector3 builderPosition = builder.transform.position;

        Vector3? validPosition = FindValidBuildingPosition(builderPosition, castlePrefab, 6f, 1f);
        if (validPosition.HasValue)
        {
            Debug.Log("[BotPlayer] Found valid building position: " + validPosition.Value);

            rtsController.BuildingManager.botMoveBuilders(singleBuilderList, validPosition.Value, validPosition.Value);
        }
        else
        {
            Debug.Log("[BotPlayer] Could not find a valid building position near builder.");
        }
    }

    private Vector3? FindValidBuildingPosition(Vector3 centerPosition, GameObject buildingPrefab, float searchRadius = 5f, float step = 1f)
    {
        Vector2 prefabSize = buildingPrefab.GetComponent<SpriteRenderer>().bounds.size;

        for (float x = -searchRadius; x <= searchRadius; x += step)
        {
            for (float y = -searchRadius; y <= searchRadius; y += step)
            {
                Vector3 checkPosition = new Vector3(centerPosition.x + x, centerPosition.y + y, 0f);

                Collider2D[] colliders = Physics2D.OverlapBoxAll(
                    checkPosition,
                    prefabSize,
                    0f
                );

                bool isValid = true;
                foreach (var collider in colliders)
                {
                    // skip ground or ghost, but disallow any other objects
                    if (collider.tag != "Ground" && collider.tag != "Ghost")
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    return checkPosition;
                }
            }
        }

        return null; // No valid position found
    }

}

