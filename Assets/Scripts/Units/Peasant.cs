using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Peasant : UnitRTS
{
    protected override float moveSpeed { get; set; } = 9f;
    protected override float maxHp => 5f;

    private Coroutine buildCouroutine;
    private bool isBuilding;
    private EnvironmentResource lastGatheredResource;
    private Resource.ResourceType lastGatheredType;

    public override float health { get; set; } = 7f;
    public virtual int maxCarryCapacity => 10;
    public override float spawnTime => 6f;
    public Resource carriedResource;
    public List<SpellSO> buildingButtons;

    public Peasant()
    {
        RequiredResources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, 15 },
            { ResourceType.Wood, 0 }
        };
    }

    protected override void Start()
    {
        base.Start();
        attackType = AttackType.Melee;
        isBuilding = false;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButton(1))
        {
            StopBuildingProcess();
            List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;
            if (selectedUnits.Contains(this))
            {
                Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                peasantRightClick(clickPosition);
            }
        }
    }

    public void StartBuildingProcess(GameObject building)
    {
        if (buildCouroutine == null)
        {
            RTS_building buildingComponent = building.GetComponent<RTS_building>();

            if (buildingComponent.health < buildingComponent.maxHealth) {
                isBuilding = true;
                buildCouroutine = StartCoroutine(Build(buildingComponent));
            }
        }
    }

    public void SetCarriedResource(Resource.ResourceType type, int amount)
    {
        if (carriedResource == null)
        {
            carriedResource = new Resource(type, amount);
        }
        else
        {
            carriedResource.amount += amount;
        }

        carriedResource.amount = Mathf.Min(carriedResource.amount, maxCarryCapacity);
    }

    public void ClearCarriedResource()
    {
        carriedResource = null;
    }

    private IEnumerator Build(RTS_building building)
    {
        while (building.health < building.maxHealth)
        {
            building.health += Time.deltaTime;
            building.healthBar.updateHealthBar(building.health, building.maxHealth);
            yield return null;
        }

        building.finished = true;
        if(buildCouroutine != null)
        {
            StopBuildingProcess();
        }
    }

    private void StopBuildingProcess()
    {
        if(buildCouroutine != null)
        {
            StopCoroutine(buildCouroutine);
            isBuilding = false;
            buildCouroutine = null;
        }
    }

    private void peasantRightClick(Vector3 clickPosition)
    {
        lastGatheredResource = null;
        RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);
        if (hit.collider != null)
        {
            GameObject clickedObject = hit.collider.gameObject;
            MoveTo(clickPosition);

            if (clickedObject.CompareTag("Building"))
            {
                RTS_building clickedBuilding = clickedObject.GetComponent<RTS_building>();
                if(clickedBuilding.team == this.team)
                {
                    StartCoroutine(constructionPath(clickedObject));
                }
            } else if (clickedObject.CompareTag("EnvironmentResource"))
            {
                EnvironmentResource clickedResource = clickedObject.GetComponent<EnvironmentResource>();
                if(clickedResource != null)
                {
                    StartCoroutine(gatherResource(clickedResource, clickedResource.GetProvidedResourceType()));
                }
            }

        } else
        {
            if (isBuilding)
            {
                StopBuildingProcess();
            }
        }
    }

    public IEnumerator constructionPath(GameObject buildingObject)
    {
        while (!HasReachedDestination())
        {
            yield return null;
        }


        if (buildingObject.GetComponent<Castle>() != null && carriedResource != null)
        {
            peasantTransferResource();
        }
        else if(buildingObject.GetComponent<GoldenMine>() != null && buildingObject.GetComponent<RTS_building>().finished)
        {
            if(carriedResource == null || carriedResource.amount < maxCarryCapacity)
            {
                peasantSendToMine(buildingObject);
            }
        }

        StartBuildingProcess(buildingObject);
    }

    private IEnumerator gatherResource(EnvironmentResource target, Resource.ResourceType gatheredResource)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;

        lastGatheredResource = target;
        lastGatheredType = gatheredResource;

        while (target.health > 0)
        {
            while (!HasReachedDestination())
            {
                yield return null;
            }

            if (IsAtMaxCarryCapacity())
            {
                StartCoroutine(DeliverAndResume());
                yield break; 
            }

            float attackDelay = 1 / attackSpeed;
            yield return new WaitForSeconds(attackDelay);

            if (target.health > 0)
            {
                target.takeDamage(attackDamage, this);
                SetCarriedResource(gatheredResource, 1);
                Debug.Log($"I am carrying {carriedResource.type} + {carriedResource.amount}");
            }
            else
            {
                break;
            }
        }

        isAttacking = false;
    }

    private void peasantTransferResource()
    {
        Dictionary<Resource.ResourceType, int> transferredResource = new Dictionary<Resource.ResourceType, int>
            {
                { carriedResource.type, carriedResource.amount }
            };

        this.owner.ChangePlayerResources(transferredResource, "+");
        ClearCarriedResource();
    }

    private void peasantSendToMine(GameObject buildingObject)
    {
        SetCarriedResource(ResourceType.Gold, 0);
        buildingObject.GetComponent<GoldenMine>().AddWorkerToMine(this);
        this.gameObject.SetActive(false);
        lastGatheredResource = buildingObject.GetComponent<EnvironmentResource>();
        lastGatheredType = ResourceType.Gold;
    }

    private bool IsAtMaxCarryCapacity()
    {
        return carriedResource != null && carriedResource.amount >= maxCarryCapacity;
    }

    public IEnumerator DeliverAndResume()
    {
        GameObject nearestCastle = FindNearestBuilding<Castle>();
        if (nearestCastle == null)
        {
            Debug.LogWarning("No castle found to deliver resource.");
            yield break;
        }

        isAttacking = false;
        MoveTo(nearestCastle.transform.position);

        while (!HasReachedDestination())
            yield return null;

        peasantTransferResource();

        if (lastGatheredResource != null)
        {
            Debug.Log("Returning to gather after delivery");
            MoveTo(lastGatheredResource.transform.position);

            while (!HasReachedDestination())
                yield return null;

            // Restart gathering
            if(lastGatheredType == ResourceType.Gold)
            {
                peasantSendToMine(lastGatheredResource.gameObject);
            }
            else
            {
                StartCoroutine(gatherResource(lastGatheredResource, lastGatheredType));
            }
        }
    }

    private GameObject FindNearestBuilding<T>() where T : RTS_building
    {
        T[] allBuildings = FindObjectsOfType<T>();
        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (T building in allBuildings)
        {
            if (building.team != this.team) continue;

            float dist = Vector3.Distance(transform.position, building.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = building.gameObject;
            }
        }

        return nearest;
    }
}
