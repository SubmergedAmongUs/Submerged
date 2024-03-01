using HarmonyLib;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Floors.Patches;

[HarmonyPatch]
public static class CameraTeleportPatches
{
    [HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
    [HarmonyPrefix]
    public static void TeleportCameraOnDifferentFloorPatch(FollowerCamera __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        if (!__instance.Target) return;

        Vector3 targetPos = __instance.Target.transform.position;
        if (Vector2.Distance(__instance.centerPosition, __instance.Target.transform.position) > 40f) __instance.centerPosition = targetPos;
    }
}
