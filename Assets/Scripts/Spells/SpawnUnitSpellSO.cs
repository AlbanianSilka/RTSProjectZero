using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

// [CreateAssetMenu(fileName = "SpawnUnitSpell", menuName = "Spells/Spawn Unit")]
// public class SpawnUnitSpellSO : SpellSO
// {
//     public UnitRTS unitToTrain;
//
//     public override void Cast(RTS_controller controller, RTS_building building)
//     {
//         if (unitToTrain == null || building == null)
//         {
//             UnityEngine.Debug.LogError("Unit prefab or building is null in SpawnUnitSpell.");
//             return;
//         }
//
//         if (unitToTrain.CanBeTrained(controller.owner))
//         {
//             building.AddUnitToQueue(unitToTrain);
//         }
//         else
//         {
//             UnityEngine.Debug.Log("Not enough resources to train " + unitToTrain);
//         }
//     }
//}
