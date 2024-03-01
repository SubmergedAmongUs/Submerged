using System;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Submerged.Extensions;
using Submerged.Map;
using Submerged.Minigames.CustomMinigames.FixWiring.MonoBehaviours;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.FixWiring.Patches;

[HarmonyPatch]
public static class KcegTriggerPatches
{
    [HarmonyPatch(typeof(IntRange), nameof(IntRange.FillRandomRange))]
    [HarmonyPrefix]
    public static bool AdjustWiresPatch([HarmonyArgument(0)] Il2CppStructArray<sbyte> array)
    {
        try
        {
            if (!ShipStatus.Instance.IsSubmerged()) return true;
            if (!KcegListener.Instance || !KcegListener.Instance.triggered || array.Length != 8) return true;

            PlayerPrefs.SetInt(KcegListener.PLAYER_PREFS_KEY, 2);

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = (sbyte) i;
            }

            SoundManager.Instance.PlaySound(SubmarineStatus.instance.minigameProperties.audioClips[5], false, 1.5f);

            return false;
        }
        catch (Exception e)
        {
            Error("Caught exception in IntRange patch");
            Error(e);

            return true;
        }
    }
}
