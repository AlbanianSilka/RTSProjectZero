using UnityEngine;

[CreateAssetMenu(fileName = "UnitRegistry", menuName = "RTS/UnitRegistry")]
public class UnitRegistry : ScriptableObject
{
    public GameObject footmanPrefab;
    public GameObject archerPrefab;
    public GameObject builderPrefab;

    public GameObject GetPrefabByName(string name)
    {
        name = name.ToLower();

        return name switch
        {
            "footman" => footmanPrefab,
            "archer" => archerPrefab,
            "builder" => builderPrefab,
            _ => null
        };
    }
}
