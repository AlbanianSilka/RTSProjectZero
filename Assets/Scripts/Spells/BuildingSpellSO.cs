using System.Collections.Generic;
using UnityEngine;

namespace Spells
{
	[CreateAssetMenu(fileName = "BuildingSpellSO", menuName = "Spells/BuildingSpellSO")]
	public class BuildingSpellSO : SpellSO
	{
		//public GameObject buildingPrefab;
		[SerializeField] private List<CastBuildingSpellSO> _buildingSpells = new();

		public override void Cast(RTS_controller controller)
		{
			UI_controller.UpdateSkills(_buildingSpells.ConvertAll(spell => (SpellSO)spell));
		}
	}
}