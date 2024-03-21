using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class progress_button : MonoBehaviour
{
    protected RTS_controller rtsController;

    public int buttonIndex;

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    public void cancelTraining()
    {
        rtsController = FindObjectOfType<RTS_controller>();
        rtsController.selectedBuilding.unitsQueue.RemoveAt(buttonIndex);
        Destroy(gameObject);
        UI_controller.handleMiddleSection(rtsController.selectedBuilding.unitsQueue, rtsController.progressButtonPrefab);
    }
}
