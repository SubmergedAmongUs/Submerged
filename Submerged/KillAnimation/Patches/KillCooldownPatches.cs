using HarmonyLib;

namespace Submerged.KillAnimation.Patches;

[HarmonyPatch]
public static class KillCooldownPatches
{
    public static bool PreventReset { get; set; }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static bool PreventResetIfOxygenDeathFailedPatch(PlayerControl __instance) => !PreventReset || !__instance.AmOwner;
}
