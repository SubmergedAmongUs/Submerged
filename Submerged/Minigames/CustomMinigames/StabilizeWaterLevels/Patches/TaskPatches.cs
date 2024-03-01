using HarmonyLib;
using Il2CppSystem.Text;
using Submerged.Extensions;
using Submerged.Localization.Strings;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.StabilizeWaterLevels.Patches;

[HarmonyPatch]
public static class TaskPatches
{
    [HarmonyPatch(typeof(ReactorTask), nameof(ReactorTask.AppendTaskText))]
    [HarmonyPrefix]
    public static bool AppendTaskTextPatch(ReactorTask __instance, [HarmonyArgument(0)] StringBuilder sb)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return true;

        __instance.even = !__instance.even;
        Color color = __instance.even ? Color.yellow : Color.red;

        sb.AppendLine($"{color.ToTextColor()}" +
                      Tasks.StabilizeWaterLevels +
                      $" {(int) __instance.reactor.Countdown} " +
                      $"({__instance.reactor.UserCount}/{2})" +
                      $"{Color.white.ToTextColor()}");

        foreach (ArrowBehaviour t in __instance.Arrows)
        {
            t.image.color = color;
        }

        return false;
    }

    [HarmonyPatch(typeof(ReactorTask), nameof(ReactorTask.Initialize))]
    [HarmonyPostfix]
    public static void HasLocationPatch(ReactorTask __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        __instance.HasLocation = true;
    }
}
