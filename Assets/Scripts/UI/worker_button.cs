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
        selectedMine = rtsController.selectedBuilding.GetComponent<GoldenMine>();
    }

    public void freeWorker()
    {
        selectedMine.removeWorkerByIndex(buttonIndex);

        UI_controller.handleMineMiddle(rtsController.selectedBuilding.GetComponent<GoldenMine>().workers, rtsController.workerButtonPrefab);
    }
}
