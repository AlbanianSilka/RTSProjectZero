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
        BuildBasicSquad,
        StartBasicAttack
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
        StartCoroutine(UpdateFriendlyUnitsRoutine());
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

    private IEnumerator UpdateFriendlyUnitsRoutine()
    {
        while (true)
        {
            FindFriendlyUnits();
            yield return new WaitForSeconds(10f); // update every 10 seconds
        }
    }

    private void FindFriendlyUnits()
    {
        friendlyUnits.Clear();
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

            case BotTask.StartBasicAttack:
                basicAttack();
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

    private void basicAttack()
    {
        List<Footman> attackingFootmen = new();
        // for a basic attack bot would need some start army
        List<Footman> availableFootmen = friendlyUnits.OfType<Footman>().ToList();
        if (availableFootmen.Count < 3)
        {
            return;
        }

        if (tasksInProgress.Contains(BotTask.StartBasicAttack))
        {
            Debug.Log("[BotPlayer] Already trying to attack.");
            return;
        }

        if (!tasksInProgress.Contains(BotTask.StartBasicAttack))
        {
            tasksInProgress.Add(BotTask.StartBasicAttack);
        }

        Debug.Log("[BotPlayer] Starting basic attack!");

        GameObject targetObject = FindClosestEnemyTarget();
        if (targetObject == null)
        {
            Debug.LogWarning("[BotPlayer] No enemy target found.");
            return;
        }

        IAttackable targetAttackable = targetObject.GetComponent<IAttackable>();
        if (targetAttackable == null)
        {
            Debug.LogWarning("[BotPlayer] Target does not implement IAttackable.");
            return;
        }

        foreach (var footman in availableFootmen)
        {
            if (!attackingFootmen.Contains(footman))
                attackingFootmen.Add(footman);

            Debug.Log("Set new target");
            footman.StartCoroutine(footman.attackPath(targetAttackable, targetObject));
        }

        StartCoroutine(MonitorAttackers(attackingFootmen));
    }

    private GameObject FindClosestEnemyTarget()
    {
        GameObject closestTarget = null;
        float closestDistance = float.MaxValue;

        // Find all UnitRTS in scene
        UnitRTS[] allUnits = GameObject.FindObjectsOfType<UnitRTS>();
        foreach (var unit in allUnits)
        {
            if (unit.team != this.team && unit.health > 0)
            {
                float dist = Vector3.Distance(transform.position, unit.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestTarget = unit.gameObject;
                }
            }
        }

        // If no enemy units found, look for buildings
        if (closestTarget == null)
        {
            RTS_building[] allBuildings = GameObject.FindObjectsOfType<RTS_building>();
            foreach (var building in allBuildings)
            {
                if (building.team != this.team && building.health > 0)
                {
                    float dist = Vector3.Distance(transform.position, building.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestTarget = building.gameObject;
                    }
                }
            }
        }

        return closestTarget;
    }

    private IEnumerator MonitorAttackers(List<Footman> attackingFootmen)
    {
        while (true)
        {
            // Clean up dead/null footmen
            attackingFootmen = attackingFootmen.Where(f => f != null).ToList();

            foreach (var footman in attackingFootmen)
            {
                if (!footman.isAttacking)
                {
                    GameObject newTarget = FindClosestEnemyTarget();

                    if (newTarget != null)
                    {
                        var attackable = newTarget.GetComponent<IAttackable>();
                        if (attackable != null)
                        {
                            Debug.Log("[BotPlayer] Re-assigning target for idle footman.");
                            footman.StartCoroutine(footman.attackPath(attackable, newTarget));
                        }
                    }
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }
}

