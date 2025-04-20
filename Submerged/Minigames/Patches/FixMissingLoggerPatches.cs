using HarmonyLib;

namespace Submerged.Minigames.Patches;

[HarmonyPatch]
public static class MinigameBeginPatch
{
    private static readonly Logger _logger = new(Logger.Category.Gameplay, "Minigame");

    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    [HarmonyPrefix]
    public static void Prefix(Minigame __instance)
    {
        __instance.logger ??= _logger;
    }
}
