using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peasant : UnitRTS
{
    protected override float moveSpeed => 9f;

    private Coroutine buildCouroutine;
    private bool isBuilding;
    private RTS_controller rtsController;

    private void Awake()
    {
        rtsController = FindObjectOfType<RTS_controller>();
    }

    protected override void Start()
    {
        base.Start();
        isBuilding = false;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButton(1))
        {
            List<UnitRTS> selectedUnits = rtsController.selectedUnitRTSList;
            if (selectedUnits.Contains(this))
            {
                Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                HandleRightClick(clickPosition);
            }
        }
    }

    public void StartBuildingProcess(GameObject building)
    {
        if (buildCouroutine == null)
        {
            isBuilding = true;
            RTS_building buildingComponent = building.GetComponent<RTS_building>();
            buildCouroutine = StartCoroutine(Build(buildingComponent));
        }
    }

    private IEnumerator Build(RTS_building building)
    {
        while (building.health < building.maxHealth)
        {
            building.health += Time.deltaTime;
            yield return null;
        }

        building.finished = true;
        StopBuildingProcess();
    }

    private void StopBuildingProcess()
    {
        StopCoroutine(buildCouroutine);
        isBuilding = false;
        buildCouroutine = null;
    }

    private void HandleRightClick(Vector3 clickPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);
        if (hit.collider != null)
        {
            GameObject clickedObject = hit.collider.gameObject;
            if (clickedObject.CompareTag("Building"))
            {
                StartCoroutine(constructionPath(clickedObject));
            }
        } else
        {
            if (isBuilding)
            {
                StopBuildingProcess();
            }
        }
    }

    private IEnumerator constructionPath(GameObject buildingObject)
    {
        while (!HasReachedDestination())
        {
            yield return null;
        }

        StartBuildingProcess(buildingObject);
    }
}
