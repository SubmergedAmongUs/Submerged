using HarmonyLib;

namespace Submerged.Floors.Patches;

[HarmonyPatch]
public static class EnsureFloorHandlerPatches
{
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    [HarmonyPrefix]
    public static void EnsureFloorHandlerOnCustomNetworkTransformPatch(CustomNetworkTransform __instance)
    {
        FloorHandler.GetFloorHandler(__instance);
    }
}
