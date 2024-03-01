using HarmonyLib;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class VersionShowerPatches
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void PingTrackerPatch(PingTracker __instance)
    {
        __instance.text.text += $"\n<size=50%>{SubmergedPlugin.VersionText}</size>";
    }
}
