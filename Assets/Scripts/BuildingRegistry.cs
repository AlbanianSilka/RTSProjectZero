using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingRegistry : MonoBehaviour
{
    public static BuildingRegistry Instance { get; private set; }

    private Dictionary<string, BuildingData> buildingLookup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        LoadBuildingData();
    }

    private void LoadBuildingData()
    {
        // Auto-load all BuildingData assets from the folder
        BuildingData[] allBuildings = Resources.LoadAll<BuildingData>("ScriptableObjects/Buildings");
        buildingLookup = allBuildings.ToDictionary(b => b.buildingID);
    }

    public GameObject GetBuildingPrefab(string buildingID)
    {
        Debug.Log("We entered here");
        if (buildingLookup.TryGetValue(buildingID, out var data))
        {
            return data.prefab;
        }

        Debug.LogError("No building found with ID: " + buildingID);
        return null;
    }

    public BuildingData GetBuildingData(string buildingID)
    {
        Debug.Log(buildingLookup);
        buildingLookup.TryGetValue(buildingID, out var data);
        return data;
    }
}
