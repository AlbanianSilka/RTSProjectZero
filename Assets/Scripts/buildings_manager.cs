using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildings_manager : MonoBehaviour
{
    public GameObject buildingPrefab;

    private bool isPlacingBuilding = false;

    public void StartBuilding()
    {
        isPlacingBuilding = true;

        SpriteRenderer buildingSpriteRenderer = buildingPrefab.GetComponent<SpriteRenderer>();
        Texture2D buildingTexture = buildingSpriteRenderer.sprite.texture;
        Cursor.SetCursor(buildingSpriteRenderer.sprite.texture, Vector2.zero, CursorMode.Auto);
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
