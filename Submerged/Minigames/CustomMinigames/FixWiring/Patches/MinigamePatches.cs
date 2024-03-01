using System.Linq;
using HarmonyLib;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Minigames.CustomMinigames.FixWiring.Patches;

[HarmonyPatch]
public static class MinigamePatches
{
    [HarmonyPatch(typeof(WireMinigame), nameof(WireMinigame.Begin))]
    [HarmonyPrefix]
    public static void ExpandTo8Patch(WireMinigame __instance)
    {
        int wireCount = __instance.transform.Find("RightWires").childCount;

        __instance.ActualWires = new sbyte[wireCount];
        WireMinigame.colors = new[] { Color.red, new Color(0.15f, 0.15f, 1f, 1f), Color.yellow, Color.magenta, Color.green, Color.white, Color.cyan, Color.gray }.Take(wireCount).ToArray().ShuffleCopy();
        __instance.ExpectedWires = new sbyte[wireCount];
    }
}
