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

    public void startBuilding()
    {
        isPlacingBuilding = true;
        CreateeGhostBuilding();
    }

    private void CancelBuilding()
    {
        isPlacingBuilding = false;
        destroyGhostBuilding();
    }

    private void CreateeGhostBuilding()
    {
        ghostBuildingInstance = new GameObject("GhostBuilding");
        SpriteRenderer spriteRenderer = ghostBuildingInstance.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = buildingPrefab.GetComponent<SpriteRenderer>().sprite;
        spriteRenderer.sortingLayerName = buildingPrefab.GetComponent<SpriteRenderer>().sortingLayerName;
    }

    private void destroyGhostBuilding()
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
            updateGhostBuildingPosition();
        }
    }

    private void updateGhostBuildingPosition()
    {
        if (ghostBuildingInstance == null)
        {
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        ghostBuildingInstance.transform.position = mousePosition;
    }

    // ####################################################################################

    //public GameObject buildingPrefab;

    //private bool isPlacingBuilding = false;

    //public void StartBuilding()
    //{
    //    isPlacingBuilding = true;

    //    RTS_building buildingScript = buildingPrefab.GetComponent<RTS_building>();
    //    if (buildingScript != null && buildingScript.canBuild != null)
    //    {
    //        changeCursorSprite("canBuild", buildingScript);
    //    } else
    //    {
    //        Debug.Log("You probably forgot to add red and green sprites to the building.");
    //    }
    //}

    //private void Update()
    //{
    //    if (isPlacingBuilding)
    //    {
    //        RTS_building buildingScript = buildingPrefab.GetComponent<RTS_building>();
    //        if (CheckCollisions())
    //        {
    //            changeCursorSprite("cannotBuild", buildingScript);
    //        } else if (buildingScript != null && buildingScript.canBuild != null)
    //        {
    //            changeCursorSprite("canBuild", buildingScript);
    //        }

    //        if (Input.GetMouseButtonDown(0) && !(CheckCollisions()))
    //        {
    //            PlaceBuilding();
    //        }
    //        else if (Input.GetMouseButtonDown(1))
    //        {
    //            CancelBuilding();
    //        }
    //    }
    //}

    //private bool CheckCollisions()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

    //    if (hit.collider != null && !hit.collider.CompareTag("Ground"))
    //    {
    //        return true;
    //    }

    //    return false;
    //}

    //private void PlaceBuilding()
    //{
    //    Vector3 buildPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    buildPosition.z = 0f;

    //    GameObject newBuilding = Instantiate(buildingPrefab, buildPosition, Quaternion.identity);

    //    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    //    isPlacingBuilding = false;
    //}

    //private void CancelBuilding()
    //{
    //    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    //    isPlacingBuilding = false;
    //}

    //private void changeCursorSprite(string spriteType, RTS_building buildingScript)
    //{
    //    Texture2D buildTexture = null;
    //    if (spriteType == "canBuild")
    //    {
    //        buildTexture = buildingScript.canBuild.texture;
    //    }
    //    else if (spriteType == "cannotBuild")
    //    {
    //        buildTexture = buildingScript.cannotBuild.texture;
    //    }
    //    else
    //    {
    //        throw new ArgumentException("Invalid spriteType: " + spriteType);
    //    }
    //    float xspot = buildTexture.width / 3;
    //    float yspot = buildTexture.height / 3;
    //    Vector2 hotSpot = new Vector2(xspot, yspot);
    //    Cursor.SetCursor(buildTexture, hotSpot, CursorMode.ForceSoftware);
    //}
}
