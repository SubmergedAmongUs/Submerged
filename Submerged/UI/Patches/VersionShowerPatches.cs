using HarmonyLib;
using Submerged.Enums;
using Submerged.Extensions;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class VersionShowerPatches
{
    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    [HarmonyPostfix]
    [HarmonyPriority(int.MinValue)]
    public static void PingTrackerPatch(PingTracker __instance)
    {
        if (ShipStatus.Instance.IsSubmerged() || (LobbyBehaviour.Instance && GameManager.Instance && GameManager.Instance.LogicOptions?.MapId == (byte)CustomMapTypes.Submerged))
        {
            if (!__instance.text.text.EndsWith("\n")) __instance.text.text += "\n";
            __instance.text.text += $"<size=50%>{SubmergedPlugin.VersionText}</size>";
        }
    }
}
