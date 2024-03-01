using HarmonyLib;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Floors.Patches;

[HarmonyPatch]
public static class TeleportPetPatches
{
    [HarmonyPatch(typeof(PetBehaviour), nameof(PetBehaviour.Update))]
    [HarmonyPrefix]
    public static void TeleportPetWhenFloorChangedPatch(PetBehaviour __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        if (!__instance.TargetPlayer) return;
        Vector3 petPos = __instance.transform.position;
        Vector3 myPos = __instance.TargetPlayer.transform.position;

        if (Mathf.Abs(myPos.y - petPos.y) > FloorHandler.MAP_OFFSET * 0.6f)
        {
            petPos.y += myPos.y > FloorHandler.MAP_OFFSET ? FloorHandler.MAP_OFFSET : -FloorHandler.MAP_OFFSET;
            __instance.transform.position = petPos;
        }
    }
}
