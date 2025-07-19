using UnityEngine;

namespace Spells
{
	[CreateAssetMenu(fileName = "CastBuildingSpellSO", menuName = "Spells/CastBuildingSpellSO")]
	public class CastBuildingSpellSO : SpellSO
	{
		[SerializeField] private GameObject _buildingPrefab;
		
		public override void Cast(RTS_controller controller)
		{
			if (_buildingPrefab == null)
			{
				UI_controller.UpdateSkills();
				return;
			}
			
			controller.BuildingManager.buildingPrefab = _buildingPrefab;
	        controller.BuildingManager.startBuilding();
		}
	}
}