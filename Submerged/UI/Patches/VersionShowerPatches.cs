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
        if (!__instance.text.text.EndsWith("\n")) __instance.text.text += "\n";
        __instance.text.text += $"<size=50%>{SubmergedPlugin.VersionText}</size>";
    }
}
