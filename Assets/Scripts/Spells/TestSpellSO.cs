using UnityEngine;

namespace Spells
{
	[CreateAssetMenu(fileName = "TestSpellSO", menuName = "Spells/TestSpellSO")]
	public class TestSpellSO : SpellSO
	{
		public override void Cast(RTS_controller controller)
		{
			Debug.LogError($"Casted spell {name}");
		}
	}
}