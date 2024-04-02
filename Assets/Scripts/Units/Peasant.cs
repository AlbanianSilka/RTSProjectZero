using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resource;

public class Peasant : UnitRTS
{
    protected virtual int maxCarryCapacity { get; set; } = 10;

    protected override float moveSpeed { get; set; } = 9f;
    protected override float health { get; set; } = 7f;
    protected override float maxHp => 5f;

    private Coroutine buildCouroutine;
    private bool isBuilding;

    public override float spawnTime => 6f;
    public Resource carriedResource;
    public List<GameObject> buildingButtons = new List<GameObject>();

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
        isBuilding = false;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButton(1))
        {
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
        StopCoroutine(buildCouroutine);
        isBuilding = false;
        buildCouroutine = null;
    }

    private void peasantRightClick(Vector3 clickPosition)
    {
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
                StartCoroutine(gatherResource(clickedResource, clickedResource.GetProvidedResourceType()));
            }

        } else
        {
            if (isBuilding)
            {
                StopBuildingProcess();
            }
        }
    }

    private IEnumerator constructionPath(GameObject buildingObject)
    {
        while (!HasReachedDestination())
        {
            yield return null;
        }


        if (buildingObject.GetComponent<Castle>() != null && carriedResource != null)
        {
            Dictionary<Resource.ResourceType, int> transferredResource = new Dictionary<Resource.ResourceType, int>
            {
                { carriedResource.type, carriedResource.amount }
            };

            this.owner.ChangePlayerResources(transferredResource, "+");
            ClearCarriedResource();
        } else if(buildingObject.GetComponent<GoldenMine>() != null && buildingObject.GetComponent<RTS_building>().finished)
        {
            buildingObject.GetComponent<GoldenMine>().AddWorkerToMine(this);
            this.gameObject.SetActive(false);
        }

        StartBuildingProcess(buildingObject);
    }

    private IEnumerator gatherResource(EnvironmentResource target, Resource.ResourceType gatheredResource)
    {
        if (isAttacking)
            yield break;

        isAttacking = true;
        while (target.health > 0)
        {
            while (!HasReachedDestination())
            {
                yield return null;
            }

            if (IsAtMaxCarryCapacity())
            {
                Debug.Log("Reached max carrying capacity");
                break; 
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

    private bool IsAtMaxCarryCapacity()
    {
        return carriedResource != null && carriedResource.amount >= maxCarryCapacity;
    }
}
