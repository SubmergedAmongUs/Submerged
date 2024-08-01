using HarmonyLib;

namespace Submerged.Minigames.Patches;

[HarmonyPatch]
public static class MinigameBeginPatch
{
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    [HarmonyPrefix]
    public static void Prefix(Minigame __instance)
    {
        __instance.logger ??= new Logger("Minigame", Logger.Level.Info, Logger.Category.Gameplay);
    }
}
