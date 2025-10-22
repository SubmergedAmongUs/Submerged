using System;
using System.Linq;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Resources;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class MapOptionsButtonPatches
{
    private static readonly Lazy<MapIconByName> _optionsIcon = new(() => new MapIconByName
    {
        Name = CustomMapNames.Submerged,
        MapIcon = ResourceManager.spriteCache["OptionsIcon"],
        MapImage = ResourceManager.spriteCache["OptionsBG"],
        NameImage = ResourceManager.spriteCache["OptionsLogo"]
    });

    private static readonly Lazy<MapIconByName> _gameIcon = new(() => new MapIconByName
    {
        Name = CustomMapNames.Submerged,
        MapIcon = ResourceManager.spriteCache["Logo"]
    });

    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SetupMapButtons))]
    [HarmonyPrefix]
    public static void AddToGameOptionsUI(GameOptionsMapPicker __instance)
    {
        if (__instance.AllMapIcons.ToArray().Any(x => x.Name == CustomMapNames.Submerged)) return;
        __instance.AllMapIcons.Insert(4, _optionsIcon.Value);
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPrefix]
    public static void AddToOptionsDisplay(GameStartManager __instance)
    {
        if (__instance.AllMapIcons.ToArray().Any(x => x.Name == CustomMapNames.Submerged)) return;
        __instance.AllMapIcons.Insert(4, _gameIcon.Value);
    }

    [HarmonyPatch(typeof(MapSelectionGameSetting), nameof(MapSelectionGameSetting.GetValueString))]
    [HarmonyPrefix]
    public static void AddToActualOptions(MapSelectionGameSetting __instance)
    {
        if (__instance.Values.Length < 6)
        {
            __instance.Values = __instance.Values.Append(CustomStringNames.Submerged).ToArray();
        }
    }
}

