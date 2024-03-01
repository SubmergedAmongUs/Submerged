using HarmonyLib;
using Reactor.Utilities.Extensions;
using Submerged.Enums;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.SpotWhaleShark.Patches;

[HarmonyPatch]
public static class HudMapIconPatches
{
    [HarmonyPatch(typeof(MapTaskOverlay), nameof(MapTaskOverlay.SetIconLocation))]
    [HarmonyPostfix]
    public static void AddComponentPatch(MapTaskOverlay __instance, [HarmonyArgument(0)] PlayerTask task)
    {
        if (task.TaskType != CustomTaskTypes.SpotWhaleShark) return;

        for (int i = 0; i < task.Locations.Count; i++)
        {
            WhaleSharkMapIcon whaleIcon = __instance.data[task.name + i].gameObject.AddComponent<WhaleSharkMapIcon>();
            whaleIcon.task = task.TryCast<WhaleSharkTask>();
            whaleIcon.UpdateIcon();
        }
    }

    [HarmonyPatch(typeof(PooledMapIcon), nameof(PooledMapIcon.Reset))]
    [HarmonyPrefix]
    public static void ResetColorPatch(PooledMapIcon __instance)
    {
        if (__instance.GetComponent<WhaleSharkMapIcon>() is { } component)
        {
            component.Destroy();
            __instance.GetComponent<SpriteRenderer>().color = new Color(1, 0.9216f, 0.0157f);
        }
    }
}
