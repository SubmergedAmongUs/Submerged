using System.Linq;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Resources;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class MapOptionsButtonPatches
{
    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.Initialize))]
    [HarmonyPrefix]
    public static void AddToGameOptionsUI(GameOptionsMapPicker __instance)
    {
        __instance.AllMapIcons.Insert(4, new MapIconByName
        {
            Name = CustomMapNames.Submerged,
            MapIcon = ResourceManager.spriteCache["OptionsIcon"],
            MapImage = ResourceManager.spriteCache["OptionsBG"],
            NameImage = ResourceManager.spriteCache["OptionsLogo"]
        });
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPrefix]
    public static void AddToOptionsDisplay(GameStartManager __instance)
    {
        __instance.AllMapIcons.Insert(4, new MapIconByName
        {
            Name = CustomMapNames.Submerged,
            MapIcon = ResourceManager.spriteCache["Logo"],
        });
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
