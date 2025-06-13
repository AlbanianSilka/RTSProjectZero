using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "RTS/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingID;         // e.g. "castle"
    public string displayName;        // e.g. "Castle"
    public GameObject prefab;         // Drag Castle.prefab here
}
