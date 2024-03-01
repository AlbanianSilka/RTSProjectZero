using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class buildings_manager : MonoBehaviour
{
    public GameObject buildingPrefab;

    private bool isPlacingBuilding = false;
    private GameObject ghostBuildingInstance;
    private bool canPlaceBuilding = false; 

    public void startBuilding()
    {
        isPlacingBuilding = true;
        CreateGhostBuilding();
    }

    private void CancelBuilding()
    {
        isPlacingBuilding = false;
        DestroyGhostBuilding();
    }

    private void CreateGhostBuilding()
    {
        ghostBuildingInstance = new GameObject("GhostBuilding");
        SpriteRenderer spriteRenderer = ghostBuildingInstance.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = buildingPrefab.GetComponent<SpriteRenderer>().sprite;
        spriteRenderer.sortingLayerName = buildingPrefab.GetComponent<SpriteRenderer>().sortingLayerName;
        ghostBuildingInstance.transform.localScale = buildingPrefab.transform.localScale;
    }

    private void DestroyGhostBuilding()
    {
        if (ghostBuildingInstance != null)
        {
            Destroy(ghostBuildingInstance);
        }
    }

    private void Update()
    {
        if (isPlacingBuilding)
        {
            UpdateGhostBuildingPosition();
            CheckBuildingPlacementValidity();

            if (Input.GetMouseButtonDown(0) && canPlaceBuilding) 
            {
                PlaceBuilding();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelBuilding();
            }
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

        canPlaceBuilding = true; // TODO: later need to be changed so the builder would start a building process

        foreach (Collider2D collider in colliders)
        {
            if (collider.tag != "Ground")
            {
                canPlaceBuilding = false; 
                break;
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

    private void PlaceBuilding()
    {
        GameObject newBuilding = Instantiate(buildingPrefab, ghostBuildingInstance.transform.position, Quaternion.identity);

        DestroyGhostBuilding();

        isPlacingBuilding = false;
    }
}
