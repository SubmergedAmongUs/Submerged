using System.Linq;
using AmongUs.QuickChat;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Extensions;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class QuickChatPatches
{
    [HarmonyPatch(typeof(QuickChatContext), nameof(QuickChatContext.GetCurrentMapID))]
    [HarmonyPrefix]
    public static bool MapIDPatch(ref MapNames __result, [HarmonyArgument(0)] ShipStatus ship)
    {
        if (!ship.IsSubmerged()) return true;

        __result = CustomMapNames.Submerged;
        return false;
    }

    [HarmonyPatch(typeof(QuickChatContext), nameof(QuickChatContext.UpdateWithCurrentLobby))]
    [HarmonyPostfix]
    public static void AddSubmergedInLobbyPatch(QuickChatContext __instance)
    {
        __instance.locations = __instance.locations.AddItem(CustomStringNames.Submerged).ToArray();
    }
}
