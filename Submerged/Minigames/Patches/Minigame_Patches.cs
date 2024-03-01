using HarmonyLib;

namespace Submerged.Minigames.Patches;

[HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
public static class MinigameBeginPatch
{
    [HarmonyPrefix]
    public static void Prefix(Minigame __instance)
    {
        __instance.logger ??= new Logger("Minigame", Logger.Level.Info, Logger.Category.Gameplay);
    }
}
