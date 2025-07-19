using UnityEngine;

namespace Spells
{
	[CreateAssetMenu(fileName = "BuildingSpellSO", menuName = "Spells/BuildingSpellSO")]
	public class BuildingSpellSO : SpellSO
	{
		public GameObject buildingPrefab;
		
		public override void Cast(RTS_controller controller)
		{
			controller.BuildingManager.buildingPrefab = buildingPrefab;
			controller.BuildingManager.startBuilding();
		}
	}
}