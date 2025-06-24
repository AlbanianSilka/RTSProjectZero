using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Resource;

public class BotPlayer : Player
{
    public UnitRegistry unitRegistry;

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();
    private List<UnitRTS> friendlyUnits = new List<UnitRTS>();
    private List<RTS_building> friendlyBuildings = new List<RTS_building>();

    private enum BotTask
    {
        BuildCastle,
        BuildBasicSquad
    }

    private List<BotTask> availableTasks;
    private HashSet<BotTask> tasksInProgress = new HashSet<BotTask>();

    public BotPlayer()
    {
        resources.Add(Resource.ResourceType.Gold, 500);
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

        availableTasks = Enum.GetValues(typeof(BotTask)).Cast<BotTask>().ToList();
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
        friendlyBuildings.Clear();

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
            foreach (var task in availableTasks)
            {
                if (!tasksInProgress.Contains(task))
                {
                    ExecuteTask(task);
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void ExecuteTask(BotTask task)
    {
        switch (task)
        {
            case BotTask.BuildCastle:
                BuildCastleTask();
                break;

            case BotTask.BuildBasicSquad:
                BuildBasicSquadTask();
                break;
        }
    }

    private bool HasBuilding<T>() where T : RTS_building
    {
        FindFriendlyBuildings();
        return friendlyBuildings.Any(b => b is T typed && typed.finished);
    }

    private void BuildCastleTask()
    {
        if (HasBuilding<Castle>())
        {
            Debug.Log("[BotPlayer] Castle already exists — skipping task.");
            tasksInProgress.Remove(BotTask.BuildCastle);
            return;
        }

        Peasant builder = GetAvailableBuilder();
        if (builder == null)
        {
            Debug.LogWarning("[BotPlayer] No available builder to construct the Castle.");
            return;
        }

        List<Peasant> singleBuilderList = new List<Peasant> { builder };

        GameObject castlePrefab = BuildingRegistry.Instance.GetBuildingPrefab("castle");
        if (castlePrefab == null)
        {
            Debug.LogError("[BotPlayer] Could not find castle prefab.");
            return;
        }

        tasksInProgress.Add(BotTask.BuildCastle);

        rtsController.BuildingManager.buildingPrefab = castlePrefab;

        Vector3 builderPosition = builder.transform.position;
        Vector3? validPosition = FindValidBuildingPosition(builderPosition, castlePrefab, 6f, 1f);

        if (validPosition.HasValue)
        {
            Debug.Log("[BotPlayer] Found valid building position: " + validPosition.Value);
            rtsController.BuildingManager.botMoveBuilders(singleBuilderList, validPosition.Value, validPosition.Value);
        }
        else
        {
            Debug.LogWarning("[BotPlayer] Could not find a valid building position near builder.");
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

    private void BuildBasicSquadTask()
    {
        if (!HasBuilding<Castle>())
        {
            Debug.Log("[BotPlayer] Cannot build squad — no Castle.");
            return;
        }

        int footmenCount = friendlyUnits.OfType<Footman>().Count();
        if (footmenCount >= 3)
        {
            tasksInProgress.Remove(BotTask.BuildBasicSquad);
            return;
        }

        if (tasksInProgress.Contains(BotTask.BuildBasicSquad))
        {
            Debug.Log("[BotPlayer] Already building a squad.");
            return;
        }

        if (!tasksInProgress.Contains(BotTask.BuildBasicSquad))
        {
            Debug.Log("[BotPlayer] Need to build an army.");
            tasksInProgress.Add(BotTask.BuildBasicSquad);
        }

        RTS_building castle = friendlyBuildings.FirstOrDefault(b => b is Castle && b.finished);
        if (castle == null) return;


        int unitsToTrain = 3 - footmenCount;
        for (int i = 0; i < unitsToTrain; i++)
        {
            // Instantiate a Footman prefab (without spawning it immediately)
            GameObject footmanPrefab = unitRegistry.GetPrefabByName("footman");
            if (footmanPrefab == null)
            {
                Debug.LogError("[BotPlayer] Could not find Footman prefab.");
                return;
            }

            UnitRTS unitToTrain = footmanPrefab.GetComponent<UnitRTS>();
            TrainUnitAtBuilding(castle, unitToTrain);
        }
    }

    public void TrainUnitAtBuilding(RTS_building building, UnitRTS unit)
    {
        if (unit.CanBeTrained(this))
        {
            if (building != null)
            {
                building.AddUnitToQueue(unit);
            }
            else
            {
                Debug.LogError("Building is null in TrainUnitAtBuilding");
            }
        }
        else
        {
            Debug.Log("[RTS_controller] Not enough resources to train unit.");
        }
    }

}

