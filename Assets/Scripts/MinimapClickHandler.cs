using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Camera minimapCamera;
    public Camera mainCamera;
    public RectTransform minimapRect;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        Vector2 normalized = Rect.PointToNormalized(minimapRect.rect, localCursor);

        float worldZ = 0f;
        float distanceToWorld = Mathf.Abs(minimapCamera.transform.position.z - worldZ);

        // Correct position on map
        Vector3 worldPos = minimapCamera.ViewportToWorldPoint(new Vector3(normalized.x, normalized.y, distanceToWorld));

        // Move main camera
        Vector3 targetPosition = worldPos;
        targetPosition.z = mainCamera.transform.position.z;
        mainCamera.transform.position = targetPosition;
    }
}

