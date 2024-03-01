using System;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Submerged.Enums;
using UnityEngine;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class SwapFungleAndSubmergedPatches
{
    [HarmonyPatch(typeof(KeyValueOption), nameof(KeyValueOption.OnEnable))]
    [HarmonyPostfix]
    public static void RerouteSavedValuePatch(KeyValueOption __instance)
    {
        if (__instance.Title != StringNames.GameMapName) return;
        if (__instance.name.Contains("(Clone)")) return;

        if (__instance.Values == null || __instance.Selected >= __instance.Values.Count) return;

        if (__instance.Values._items[__instance.Selected].Value == (int) CustomMapNames.Submerged)
        {
            __instance.Selected = __instance.Values.FindIndexOfMap(MapNames.Fungle);
        }
        else if (__instance.Values._items[__instance.Selected].Value == (int) MapNames.Fungle)
        {
            __instance.Selected = __instance.Values.FindIndexOfMap(CustomMapNames.Submerged);
        }
    }

    [HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.InitializeOptions))]
    [HarmonyPostfix]
    public static void ReorderValuesOncePatch(GameSettingMenu __instance, [HarmonyArgument(0)] Il2CppReferenceArray<Transform> items)
    {
        KeyValueOption option = items.First(i => i.name == "MapName").GetComponent<KeyValueOption>();

        int submergedIndex = option.Values.FindIndexOfMap(CustomMapNames.Submerged);
        int fungleIndex = option.Values.FindIndexOfMap(MapNames.Fungle);

        if (submergedIndex > fungleIndex)
        {
            (option.Values._items[submergedIndex], option.Values._items[fungleIndex]) = (option.Values._items[fungleIndex], option.Values._items[submergedIndex]);
        }
    }

    private static int FindIndexOfMap(this ICG.List<ICG.KeyValuePair<string, int>> self, MapNames target)
    {
        int index = 0;

        foreach (ICG.KeyValuePair<string, int> kvp in self)
        {
            if (kvp.Value == (int) target) return index;
            index++;
        }

        throw new ArgumentException($"Map {target} not found in {self}");
    }
}
