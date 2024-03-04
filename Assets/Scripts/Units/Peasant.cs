using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Peasant : UnitRTS
{
    protected override float moveSpeed => 9f;

    public void StartBuildingProcess(GameObject building)
    {
        RTS_building buildingComponent = building.GetComponent<RTS_building>();
        StartCoroutine(Build(buildingComponent));
    }

    private IEnumerator Build(RTS_building building)
    {
        while(building.health < building.maxHealth)
        {
            building.health += Time.deltaTime;
            yield return null;
        }

        building.finished = true;
    }
}
