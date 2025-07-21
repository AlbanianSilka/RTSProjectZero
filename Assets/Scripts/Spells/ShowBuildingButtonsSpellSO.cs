using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "ShowBuildingButtons", menuName = "Spells/Show Building Buttons")]
public class ShowBuildingButtonsSpellSO : SpellSO
{
    [SerializeField] private List<BuildStructureSpellSO> _buildingSpells = new();

    public override void Cast(RTS_controller controller)
    {
        UI_controller.UpdateSkills(_buildingSpells.ConvertAll(spell => (SpellSO)spell));
    }
}
