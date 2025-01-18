using HarmonyLib;

namespace Submerged.Minigames.Patches;

[HarmonyPatch]
public static class MinigameBeginPatch
{
    private static readonly Logger logger = new global::Logger("Minigame", global::Logger.Level.Info, global::Logger.Category.Gameplay);
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    [HarmonyPrefix]
    public static bool Prefix(Minigame __instance)
    {
        __instance.logger = logger;
        return true;
    }
}
