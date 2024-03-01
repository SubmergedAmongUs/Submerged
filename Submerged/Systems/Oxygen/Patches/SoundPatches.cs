using HarmonyLib;
using Submerged.Extensions;
using Submerged.Map;
using UnityEngine;

namespace Submerged.Systems.Oxygen.Patches;

[HarmonyPatch]
public static class SoundPatches
{
    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySound))]
    [HarmonyPrefix]
    public static void ReplaceO2SabotageSoundWithAirshipPatch([HarmonyArgument(0)] ref AudioClip clip, [HarmonyArgument(2)] ref float volume)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;
        if (clip != MapLoader.Skeld.SabotageSound) return;

        SubmarineOxygenSystem system = SubmarineOxygenSystem.Instance;

        if (system.recentlyActive > 0)
        {
            clip = MapLoader.Airship.SabotageSound;

            if (system.playersWithMask.Contains(PlayerControl.LocalPlayer.PlayerId))
            {
                volume = 0.2f;
            }
            else if (PlayerControl.LocalPlayer.Data.IsDead)
            {
                volume = 0.5f;
            }
            else
            {
                volume = 0.8f;
            }
        }
    }
}
