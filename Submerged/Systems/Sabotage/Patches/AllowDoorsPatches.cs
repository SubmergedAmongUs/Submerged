using HarmonyLib;
using Submerged.Map;

namespace Submerged.Systems.Sabotage.Patches;

[HarmonyPatch]
public static class AllowDoorsPatches
{
    [HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.CanUseDoors), MethodType.Getter)]
    [HarmonyPostfix]
    public static void AllowDoorsToBeUsedWithOtherSabotagesPatch(ref bool __result)
    {
        __result |= SubmarineStatus.instance;
    }
}
