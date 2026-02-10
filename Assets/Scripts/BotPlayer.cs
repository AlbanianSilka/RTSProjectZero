using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Resource;

public class BotPlayer : Player
{
    public UnitRegistry unitRegistry;

    private List<UnitRTS> friendlyUnits = new List<UnitRTS>();
    private List<RTS_building> friendlyBuildings = new List<RTS_building>();
    private List<Peasant> GoldGatherers = new();
    private List<Peasant> WoodGatherers = new();

    [SerializeField] private int desiredPeasantCount = 4;
    [SerializeField] private int desiredFootmanCount = 5;
    [SerializeField] private int workersPerResourceType = 2;
    [SerializeField] private int minimumAttackFootmen = 3;

    private bool isAttacking;

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
            FindFriendlyUnits();
            FindFriendlyBuildings();

            EnsureEconomy();
            EnsureArmy();
            TryBasicAttack();

            yield return new WaitForSeconds(1f);
        }
    }

    private void EnsureEconomy()
    {
        if (!HasBuilding<Castle>())
        {
            BuildingTask<Castle>("castle");
            return;
        }

        if (!HasBuilding<GoldenMine>())
        {
            BuildingTask<GoldenMine>("GoldenMine");
        }

        BuildUnitSquadTask<Peasant>("builder", desiredPeasantCount);
        GatheringTask(Resource.ResourceType.Gold, workersPerResourceType);
        GatheringTask(Resource.ResourceType.Wood, workersPerResourceType);
    }

    private void EnsureArmy()
    {
        if (!HasBuilding<Castle>())
            return;

        BuildUnitSquadTask<Footman>("footman", desiredFootmanCount);
    }

    private void TryBasicAttack()
    {
        if (isAttacking)
            return;

        List<Footman> availableFootmen = friendlyUnits.OfType<Footman>().ToList();
        if (availableFootmen.Count < minimumAttackFootmen)
            return;

        basicAttack(availableFootmen);
    }

    private bool HasBuilding<T>() where T : RTS_building
    {
        FindFriendlyBuildings();
        return friendlyBuildings.Any(b => b is T typed && typed.finished);
    }

    private void BuildingTask<T>(string buildingName, float searchRadius = 6f, float step = 1f)
        where T : RTS_building
    {
        if (HasBuilding<T>())
            return;

        Peasant builder = GetAvailableBuilder();
        if (builder == null)
        {
            Debug.LogWarning($"[BotPlayer] No available builder to construct {buildingName}.");
            return;
        }

        GameObject prefab = BuildingRegistry.Instance.GetBuildingPrefab(buildingName.ToLower());
        if (prefab == null)
        {
            Debug.LogError($"[BotPlayer] Could not find prefab for {buildingName}.");
            return;
        }

        rtsController.BuildingManager.buildingPrefab = prefab;

        Vector3 builderPosition = builder.transform.position;
        Vector3? validPosition = FindValidBuildingPosition(builderPosition, prefab, searchRadius, step);

        if (validPosition.HasValue)
        {
            Debug.Log($"[BotPlayer] Found valid position for {buildingName} at: {validPosition.Value}");
            rtsController.BuildingManager.MoveBuilders(new List<Peasant> { builder }, validPosition.Value);
        }
        else
        {
            Debug.LogWarning($"[BotPlayer] Could not find valid position for {buildingName} near builder.");
        }
    }

    private Vector3? FindValidBuildingPosition(Vector3 centerPosition, GameObject buildingPrefab, float searchRadius = 5f, float step = 1f)
    {
        float maxSearchRadius = 50f;
        Vector2 prefabSize = buildingPrefab.GetComponent<SpriteRenderer>().bounds.size;

        // Special case: if building is a GoldenMine, it must be placed on a Deposit
        if (buildingPrefab.GetComponent<GoldenMine>() != null)
        {
            float currentSearchRadius = searchRadius;

            while (currentSearchRadius <= maxSearchRadius)
            {
                Deposit[] allDeposits = FindObjectsOfType<Deposit>();
                Deposit nearestDeposit = null;
                float minDistance = float.MaxValue;

                foreach (var deposit in allDeposits)
                {
                    float distance = Vector3.Distance(centerPosition, deposit.transform.position);
                    if (distance <= currentSearchRadius)
                    {
                        Collider2D[] overlapping = Physics2D.OverlapBoxAll(
                            deposit.transform.position,
                            prefabSize,
                            0f
                        );

                        bool isFree = true;

                        if (isFree && distance < minDistance)
                        {
                            minDistance = distance;
                            nearestDeposit = deposit;
                            rtsController.BuildingManager.selectedDeposit = nearestDeposit;
                        }
                    }
                }

                if (nearestDeposit != null)
                    return nearestDeposit.transform.position;

                currentSearchRadius += step;
            }

            return null; // No free deposit found
        }

        // Generic search for other buildings (not GoldenMine)
        for (float x = -searchRadius; x <= searchRadius; x += step)
        {
            for (float y = -searchRadius; y <= searchRadius; y += step)
            {
                Vector3 checkPosition = new Vector3(centerPosition.x + x, centerPosition.y + y, 0f);

                Collider2D[] colliders = Physics2D.OverlapBoxAll(checkPosition, prefabSize, 0f);

                bool isValid = true;
                foreach (var collider in colliders)
                {
                    if (collider.tag != "Ground" && collider.tag != "Ghost")
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                    return checkPosition;
            }
        }

        return null; // No valid position found for general building
    }

    private void GatheringTask(Resource.ResourceType resourceType, int numberOfWorkers)
    {
        List<Peasant> idlePeasants = GetIdlePeasants();
        if (idlePeasants.Count == 0) return;

        // Currrently gathering ALL available builders even if need more
        // TODO: probably later in this case would activate a task for...
        // ... training more builders
        int actualCount = Mathf.Min(numberOfWorkers, idlePeasants.Count);
        List<Peasant> assignedPeasants = idlePeasants.Take(actualCount).ToList();

        switch (resourceType)
        {
            case Resource.ResourceType.Gold:
                GoldenMine goldMine = FindNearestFriendlyBuilding<GoldenMine>(assignedPeasants[0].transform.position);
                if (goldMine == null) return;

                foreach (Peasant peasant in assignedPeasants)
                {
                    peasant.MoveTo(goldMine.transform.position);
                    peasant.StartCoroutine(peasant.constructionPath(goldMine.gameObject));
                    GoldGatherers.Add(peasant);
                }
                break;

            case Resource.ResourceType.Wood:
                EnvironmentResource woodResource = GetNearestWoodResource(assignedPeasants[0].transform.position);
                if (woodResource == null) return;

                foreach (Peasant peasant in assignedPeasants)
                {
                    peasant.MoveTo(woodResource.transform.position);
                    peasant.StartCoroutine(peasant.gatherResource(woodResource, Resource.ResourceType.Wood));
                    WoodGatherers.Add(peasant);
                }
                break;
        }
    }

    private List<Peasant> GetIdlePeasants()
    {
        return friendlyUnits
        .Where(unit => unit is Peasant peasant &&
                       !peasant.isAttacking &&
                       !peasant.isBuilding &&
                       peasant.HasReachedDestination() &&
                       !GoldGatherers.Contains(peasant) &&
                       !WoodGatherers.Contains(peasant))
        .Cast<Peasant>()
        .ToList();
    }

    private T FindNearestFriendlyBuilding<T>(Vector3 referencePosition) where T : RTS_building
    {
        FindFriendlyBuildings();

        return friendlyBuildings
            .Where(b => b is T typed && typed.finished)
            .Cast<T>()
            .OrderBy(b => Vector3.Distance(b.transform.position, referencePosition))
            .FirstOrDefault();
    }

    private EnvironmentResource GetNearestWoodResource(Vector3 referencePosition)
    {
        GameObject[] allResources = GameObject.FindGameObjectsWithTag("EnvironmentResource");

        return allResources
            .Select(go => go.GetComponent<EnvironmentResource>())
            .Where(res => res != null && res.GetProvidedResourceType() == Resource.ResourceType.Wood)
            .OrderBy(res => Vector3.Distance(res.transform.position, referencePosition))
            .FirstOrDefault();
    }

    private void BuildUnitSquadTask<T>(string unitName, int requiredCount) where T : UnitRTS
    {
        if (!HasBuilding<Castle>())
            return;

        int currentCount = friendlyUnits.OfType<T>().Count();
        if (currentCount >= requiredCount)
            return;

        Debug.Log($"[BotPlayer] Need to build {requiredCount} {typeof(T).Name}(s).");

        RTS_building castle = friendlyBuildings.FirstOrDefault(b => b is Castle && b.finished);
        if (castle == null) return;

        int unitsToTrain = requiredCount - currentCount;
        for (int i = 0; i < unitsToTrain; i++)
        {
            GameObject unitPrefab = unitRegistry.GetPrefabByName(unitName);
            if (unitPrefab == null)
            {
                Debug.LogError($"[BotPlayer] Could not find prefab for {unitName}.");
                return;
            }

            UnitRTS unitToTrain = unitPrefab.GetComponent<UnitRTS>();
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

    private void basicAttack(List<Footman> availableFootmen)
    {
        List<Footman> attackingFootmen = new();
        isAttacking = true;

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
            attackingFootmen = attackingFootmen.Where(f => f != null).ToList();

            if (attackingFootmen.Count == 0)
            {
                isAttacking = false;
                yield break;
            }

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
