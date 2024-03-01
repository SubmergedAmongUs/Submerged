using HarmonyLib;
using Submerged.Extensions;

namespace Submerged.Map.Patches;

// TODO: Improve (maybe)

[HarmonyPatch]
public static class SubmarineStatusPatches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool CalculateLightRadiusPatch(ShipStatus __instance, [HarmonyArgument(0)] GameData.PlayerInfo player, ref float __result)
    {
        if (!__instance.IsSubmerged()) return true;

        __result = SubmarineStatus.instance.CalculateLightRadius(player);

        return false;
    }
}
