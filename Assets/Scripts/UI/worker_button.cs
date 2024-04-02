using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class worker_button : MonoBehaviour
{
    public RTS_controller rtsController;
    protected GoldenMine selectedMine;

    public int buttonIndex;

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    private void Start()
    {
        rtsController = FindObjectOfType<RTS_controller>();
        selectedMine = rtsController.selectedBuilding.GetComponent<GoldenMine>();
    }

    public void freeWorker()
    {
        GameObject selectedUnit = selectedMine.workers[buttonIndex].gameObject;

        UI_controller.handleMiddleSection(rtsController.selectedBuilding.unitsQueue, rtsController.workerButtonPrefab);
    }
}
