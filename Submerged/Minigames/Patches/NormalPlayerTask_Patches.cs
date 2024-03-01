using System;
using HarmonyLib;
using Submerged.Enums;

namespace Submerged.Minigames.Patches;

[HarmonyPatch(typeof(NormalPlayerTask), nameof(NormalPlayerTask.Initialize))]
public static class NormalPlayerTaskInitializePatch
{
    [HarmonyPrefix]
    public static bool Prefix(NormalPlayerTask __instance)
    {
        __instance.Arrow = __instance.gameObject.GetComponentInChildren<ArrowBehaviour>(true);

        return true;
    }

    [HarmonyPostfix]
    public static void Postfix(NormalPlayerTask __instance)
    {
        if (__instance.TaskType == CustomTaskTypes.OxygenateSeaPlants)
        {
            __instance.Data = BitConverter.GetBytes(UnityRandom.RandomRangeInt(0, int.MaxValue));
        }
    }
}
