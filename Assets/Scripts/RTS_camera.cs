using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTS_Camera : MonoBehaviour
{
    public float scrollSpeed = 5f;
    public float scrollZoneSize = 35f;

    // Borders for the camera
    public float mapLeftBorder = 0f;
    public float mapRightBorder = 50f;
    public float mapTopBorder = 50f;
    public float mapBottomBorder = 0f;

    void Update()
    {
        Vector3 newPosition = transform.position;

        if (Input.mousePosition.x > Screen.width - scrollZoneSize && newPosition.x < mapRightBorder)
        {
            newPosition.x += scrollSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.x < scrollZoneSize && newPosition.x > mapLeftBorder)
        {
            newPosition.x -= scrollSpeed * Time.deltaTime;
        }

        if (Input.mousePosition.y > Screen.height - scrollZoneSize && newPosition.y < mapTopBorder)
        {
            newPosition.y += scrollSpeed * Time.deltaTime;
        }
        else if (Input.mousePosition.y < scrollZoneSize && newPosition.y > mapBottomBorder)
        {
            newPosition.y -= scrollSpeed * Time.deltaTime;
        }

        // Stop camera when reach the borders
        newPosition.x = Mathf.Clamp(newPosition.x, mapLeftBorder, mapRightBorder);
        newPosition.y = Mathf.Clamp(newPosition.y, mapBottomBorder, mapTopBorder);

        transform.position = newPosition;
    }
}
