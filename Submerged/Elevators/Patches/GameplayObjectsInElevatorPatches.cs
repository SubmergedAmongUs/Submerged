using HarmonyLib;
using Submerged.Elevators.Objects;
using Submerged.Extensions;
using Submerged.Map;
using Submerged.Systems.Elevator;
using UnityEngine;

namespace Submerged.Elevators.Patches;

[HarmonyPatch]
public static class GameplayObjectsInElevatorPatches
{
    [HarmonyPatch(typeof(ShapeshifterRole), nameof(ShapeshifterRole.SetEvidence))]
    [HarmonyPrefix]
    public static bool MoveShapeshifterEvidencePatch(ShapeshifterRole __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return true;
        if (!GameManager.Instance.LogicOptions.GetShapeshifterLeaveSkin()) return false;

        ShapeshifterEvidence evidence = UnityObject.Instantiate(__instance.EvidencePrefab);
        Vector3 evidencePosition = __instance.Player.transform.position + __instance.EvidenceOffset * 0.7f;
        evidencePosition.z = evidencePosition.y / 1000f;
        evidence.transform.position = evidencePosition;
        evidence.gameObject.AddComponent<ElevatorMover>();

        return false;
    }

    [HarmonyPatch(typeof(PetBehaviour), nameof(PetBehaviour.SetMourning))]
    [HarmonyPostfix]
    public static void RemoveMourningPetPatch(PetBehaviour __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (SubmarineElevator elevator in SubmarineStatus.instance.elevators)
        {
            Vector3 position = __instance.transform.position;
            bool hitLower = elevator.lowerElevatorCollider.Value.OverlapPoint(position);
            bool hitUpper = elevator.upperElevatorCollider.Value.OverlapPoint(position);

            if (!hitLower && !hitUpper) continue;
            __instance.gameObject.SetActive(false);
            // __instance.animator.enabled = false;
            // __instance.rend.enabled = false;
        }
    }
}
