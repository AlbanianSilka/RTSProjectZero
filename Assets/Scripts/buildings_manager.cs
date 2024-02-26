using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class buildings_manager : MonoBehaviour
{
    public GameObject buildingPrefab;

    private bool isPlacingBuilding = false;

    public void StartBuilding()
    {
        isPlacingBuilding = true;

        RTS_building buildingScript = buildingPrefab.GetComponent<RTS_building>();
        if (buildingScript != null && buildingScript.canBuild != null)
        {
            Texture2D buildTexture = buildingScript.canBuild.texture;
            float xspot = buildTexture.width / 3;
            float yspot = buildTexture.height / 3;
            Vector2 hotSpot = new Vector2(xspot, yspot);
            Cursor.SetCursor(buildTexture, hotSpot, CursorMode.ForceSoftware);
        } else
        {
            Debug.Log("You probably forgot to add red and green sprites to the building.");
        }
    }

    private void Update()
    {
        if (isPlacingBuilding)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelBuilding();
            }
        }
    }

    private void PlaceBuilding()
    {
        Vector3 buildPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        buildPosition.z = 0f;

        GameObject newBuilding = Instantiate(buildingPrefab, buildPosition, Quaternion.identity);

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        isPlacingBuilding = false;
    }

    private void CancelBuilding()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        isPlacingBuilding = false;
    }
}
