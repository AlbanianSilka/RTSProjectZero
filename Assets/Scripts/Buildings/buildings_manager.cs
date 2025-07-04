using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UIElements;

public class buildings_manager : MonoBehaviour
{
    public GameObject buildingPrefab;

    protected internal bool isPlacingBuilding { get; set; } = false;

    private RTS_controller rtsController;
    private static GameObject ghostBuildingInstance;
    private static bool canPlaceBuilding = false;
    private Deposit selectedDeposit;

    private void Start()
    {
        rtsController = GetComponentInParent<RTS_controller>();
    }

    private void Update()
    {
        if (isPlacingBuilding)
        {
            UpdateGhostBuildingPosition();
            CheckBuildingPlacementValidity();

            if (Input.GetMouseButtonDown(0) && canPlaceBuilding)
            {
                moveBuilders();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelBuilding();
            }
        }
    }

    private bool CheckBuildingCost(Player owner)
    {
        RTS_building building = buildingPrefab.GetComponent<RTS_building>();

        if (building != null)
        {
            return building.CanBuild(owner);
        }
        else
        {
            Debug.LogError("You forgot to add a building prefab");
            return false; 
        }
    }

    public void startBuilding()
    {
        List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;
        List<UnitRTS> peasantUnits = selectedUnits.Where(unit => unit is Peasant).ToList();
        Player owner = peasantUnits.First().owner;

        if (CheckBuildingCost(owner))
        {
            isPlacingBuilding = true;
            CreateGhostBuilding();
        } else
        {
            Debug.Log("Not enough resources");
        }
    }

    private void CancelBuilding()
    {
        isPlacingBuilding = false;
        DestroyGhostBuilding();
    }

    private void CreateGhostBuilding()
    {
        DestroyGhostBuilding();

        ghostBuildingInstance = new GameObject("GhostBuilding");
        SpriteRenderer spriteRenderer = ghostBuildingInstance.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = buildingPrefab.GetComponent<SpriteRenderer>().sprite;
        spriteRenderer.sortingLayerName = buildingPrefab.GetComponent<SpriteRenderer>().sortingLayerName;
        ghostBuildingInstance.transform.localScale = buildingPrefab.transform.localScale;

        // Add a BoxCollider2D component to the ghostBuildingInstance
        BoxCollider2D boxCollider = ghostBuildingInstance.AddComponent<BoxCollider2D>();

        // Get the size of the sprite rendered by the SpriteRenderer
        Vector2 spriteSize = spriteRenderer.bounds.size;

        // Set the size of the BoxCollider2D to match the size of the sprite
        boxCollider.size = spriteSize;
        boxCollider.tag = "Ghost";
    }

    private void DestroyGhostBuilding()
    {
        if (ghostBuildingInstance != null)
        {
            Destroy(ghostBuildingInstance);
        } else
        {
            Debug.LogWarning("Ghost building is missing, nothing to destroy");
        }
    }

    private void UpdateGhostBuildingPosition()
    {
        if (ghostBuildingInstance == null)
        {
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        ghostBuildingInstance.transform.position = mousePosition;
    }

    private void CheckBuildingPlacementValidity()
    {
        if (ghostBuildingInstance == null)
        {
            return;
        }

        // Check if the ghost building overlaps with any collider
        Collider2D[] colliders = Physics2D.OverlapBoxAll(ghostBuildingInstance.transform.position,
                                                          ghostBuildingInstance.GetComponent<SpriteRenderer>().bounds.size,
                                                          0f);

        canPlaceBuilding = true;

        if(buildingPrefab.GetComponent<GoldenMine>() != null)
        {
            foreach (Collider2D collider in colliders)
            {
                if(collider.GetComponent<Deposit>() == null)
                {
                    selectedDeposit = null;
                    canPlaceBuilding = false;
                } else
                {
                    selectedDeposit = collider.GetComponent<Deposit>();
                    canPlaceBuilding = true;
                }
            }
        } else
        {
            foreach (Collider2D collider in colliders)
            {
                if (collider.tag != "Ground" && collider.tag != "Ghost")
                {
                    canPlaceBuilding = false;
                }
            }
        }

        ChangeGhostBuildingSprite(canPlaceBuilding);
    }

    private void ChangeGhostBuildingSprite(bool canPlaceBuilding)
    {
        if (ghostBuildingInstance == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer = ghostBuildingInstance.GetComponent<SpriteRenderer>();

        if (canPlaceBuilding)
        {
            spriteRenderer.sprite = buildingPrefab.GetComponent<RTS_building>().canBuild;
        }
        else
        {
            spriteRenderer.sprite = buildingPrefab.GetComponent<RTS_building>().cannotBuild;
        }
    }

    private void moveBuilders()
    {
        Vector3 buildingPosition = ghostBuildingInstance.transform.position;
        List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;
        List<UnitRTS> peasantUnits = selectedUnits.Where(unit => unit is Peasant).ToList();

        int unitCount = peasantUnits.Count;
        for (int i = 0; i < unitCount; i++)
        {
            UnitRTS unitRTS = selectedUnits[i];
            Vector3 targetPosition = CalculateTargetPosition(unitRTS.transform.position, buildingPosition);
            unitRTS.MoveTo(targetPosition);
        }
        StartCoroutine(startBuilding(buildingPosition, peasantUnits));
    }

    private IEnumerator startBuilding(Vector3 buildingPosition, List<UnitRTS> peasantUnits)
    {
        GameObject newBuilding;

        while (peasantUnits.Any(unit => !unit.HasReachedDestination()))
        {
            yield return null; 
        }

        if (buildingPrefab.GetComponent<GoldenMine>() != null)
        {
            // probably not the best way to handle it
            // need to rethink handling this coroutine in case if it got a weong building position
            if (selectedDeposit == null)
            {
                yield break;
            }

            buildingPosition = selectedDeposit.transform.position;
            newBuilding = Instantiate(buildingPrefab, buildingPosition, Quaternion.identity);
            newBuilding.GetComponent<GoldenMine>().attachedDeposit = selectedDeposit;
            selectedDeposit.gameObject.SetActive(false);
            selectedDeposit = null;
        } else
        {
            newBuilding = Instantiate(buildingPrefab, buildingPosition, Quaternion.identity);
        }

        RTS_building buildingObject = newBuilding.GetComponent<RTS_building>();
        buildingObject.team = peasantUnits.First().team;
        buildingObject.owner = peasantUnits.First().owner;
        buildingObject.health = 1;
        buildingObject.owner.ChangePlayerResources(buildingObject.GetRequiredResources(), "-");

        DestroyGhostBuilding();
        isPlacingBuilding = false;
        buildingPrefab = null;

        // Starting builders "constructing" process
        foreach (Peasant unit in peasantUnits)
        {
            unit.StartBuildingProcess(newBuilding);
        }
    }

    private Vector3 CalculateTargetPosition(Vector3 peasantPosition, Vector3 buildingPosition)
    {
        Bounds buildingBounds = ghostBuildingInstance.GetComponent<BoxCollider2D>().bounds;

        // Get the corners of the building rectangle
        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(buildingBounds.min.x, buildingBounds.min.y, 0f); // bottom-left
        corners[1] = new Vector3(buildingBounds.min.x, buildingBounds.max.y, 0f); // top-left
        corners[2] = new Vector3(buildingBounds.max.x, buildingBounds.min.y, 0f); // bottom-right
        corners[3] = new Vector3(buildingBounds.max.x, buildingBounds.max.y, 0f); // top-right

        // Find the corner with the shortest distance to the peasant unit
        float minDistance = float.MaxValue;
        Vector3 nearestCorner = Vector3.zero;
        foreach (Vector3 corner in corners)
        {
            float distance = Vector3.Distance(peasantPosition, corner);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestCorner = corner;
            }
        }

        return nearestCorner;
    }

    // ######## A copy for bots, which does not need to include ghost building & other moments for player ########

    public void botMoveBuilders(List<Peasant> builders, Vector3 buildingPosition, Vector3 targetPosition)
    {
        int unitCount = builders.Count;

        for (int i = 0; i < unitCount; i++)
        {
            Peasant peasant = builders[i];
            peasant.MoveTo(targetPosition);
        }

        StartCoroutine(botStartBuilding(buildingPosition, builders.Cast<UnitRTS>().ToList()));
    }

    public IEnumerator botStartBuilding(Vector3 buildingPosition, List<UnitRTS> peasantUnits)
    {
        GameObject newBuilding;

        while (peasantUnits.Any(unit => !unit.HasReachedDestination()))
        {
            yield return null;
        }

        newBuilding = Instantiate(buildingPrefab, buildingPosition, Quaternion.identity);
        RTS_building buildingObject = newBuilding.GetComponent<RTS_building>();
        buildingObject.team = peasantUnits.First().team;
        buildingObject.owner = peasantUnits.First().owner;
        buildingObject.health = 1;
        buildingObject.owner.ChangePlayerResources(buildingObject.GetRequiredResources(), "-");
        buildingPrefab = null;

        foreach (Peasant unit in peasantUnits)
        {
            unit.StartBuildingProcess(newBuilding);
        }
    }

    // ################################################################
}