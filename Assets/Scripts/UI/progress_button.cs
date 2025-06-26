using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class progress_button : MonoBehaviour
{
    public RTS_controller rtsController;
    protected RTS_building selectedBuilding;
    
    public int buttonIndex;
    public float maxTrainingTime;
    public healthbar_manager progressBar;
    
    private void OnDisable()
    {
        Destroy(gameObject);
    }

    private void Start()
    {
        selectedBuilding = rtsController.selectedBuilding;

        if (buttonIndex == 0)
        {
            progressBar.gameObject.SetActive(true);
        }
        else
        {
            progressBar.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if(buttonIndex == 0)
        {
            float currentTime = maxTrainingTime - selectedBuilding.remainingSpawnTime;
            this.progressBar.updateHealthBar(currentTime, maxTrainingTime);
        }
    }

    public void cancelTraining()
    {
        // changing player's resources when training canceled
        UnitRTS selectedUnit = rtsController.selectedBuilding.unitsQueue[buttonIndex];
        rtsController.owner.ChangePlayerResources(selectedUnit.GetRequiredResources(), "+");

        rtsController.selectedBuilding.unitsQueue.RemoveAt(buttonIndex);

        if(buttonIndex == 0)
        {
            rtsController.selectedBuilding.restartSpawnCoroutine();
        }

        Destroy(gameObject);
        UI_controller.handleMiddleSection(rtsController.selectedBuilding.unitsQueue, rtsController.progressButtonPrefab);
    }
}
