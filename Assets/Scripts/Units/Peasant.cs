using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peasant : UnitRTS
{
    protected override float moveSpeed { get; set; } = 9f;
    protected override float health { get; set; } = 7f;
    protected override float maxHp => 5f;

    private Coroutine buildCouroutine;
    private bool isBuilding;

    public override float spawnTime => 6f;

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
                peasantRightClick(clickPosition);
            }
        }
    }

    public void StartBuildingProcess(GameObject building)
    {
        if (buildCouroutine == null)
        {
            RTS_building buildingComponent = building.GetComponent<RTS_building>();
            if (buildingComponent.health < buildingComponent.maxHealth) {
                isBuilding = true;
                buildCouroutine = StartCoroutine(Build(buildingComponent));
            }
        }
    }

    private IEnumerator Build(RTS_building building)
    {
        while (building.health < building.maxHealth)
        {
            building.health += Time.deltaTime;
            building.healthBar.updateHealthBar(building.health, building.maxHealth);
            yield return null;
        }

        building.finished = true;
        if(buildCouroutine != null)
        {
            StopBuildingProcess();
        }
    }

    private void StopBuildingProcess()
    {
        StopCoroutine(buildCouroutine);
        isBuilding = false;
        buildCouroutine = null;
    }

    private void peasantRightClick(Vector3 clickPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(clickPosition, Vector2.zero);
        if (hit.collider != null)
        {
            GameObject clickedObject = hit.collider.gameObject;
            if (clickedObject.CompareTag("Building"))
            {
                RTS_building clickedBuilding = clickedObject.GetComponent<RTS_building>();
                if(clickedBuilding.team == this.team)
                {
                    MoveTo(clickPosition);
                    StartCoroutine(constructionPath(clickedObject));
                }
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
