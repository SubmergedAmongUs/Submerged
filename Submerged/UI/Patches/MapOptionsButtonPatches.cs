using System.Linq;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Resources;
using UnityEngine;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class MapOptionsButtonPatches
{
    [HarmonyPatch(typeof(GameOptionsMapPicker), nameof(GameOptionsMapPicker.SetupMapButtons))]
    [HarmonyPrefix]
    public static void AddToGameOptionsUI(GameOptionsMapPicker __instance)
    {
        if (__instance.AllMapIcons != null && !__instance.AllMapIcons.ToArray().Any(x => x.Name == CustomMapNames.Submerged))
        {
            __instance.AllMapIcons.Add(new MapIconByName
            {
                Name = CustomMapNames.Submerged,
                MapIcon = ResourceManager.spriteCache["OptionsIcon"],
                MapImage = ResourceManager.spriteCache["OptionsBG"],
                NameImage = ResourceManager.spriteCache["OptionsLogo"]
            });
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    [HarmonyPrefix]
    public static void AddToOptionsDisplay(GameStartManager __instance)
    {
        if (!__instance.AllMapIcons.ToArray().Any(x => x.Name == CustomMapNames.Submerged))
        {
            __instance.AllMapIcons.Add(new MapIconByName
            {
                Name = CustomMapNames.Submerged,
                MapIcon = ResourceManager.spriteCache["Logo"],
            });
        }
    }

    [HarmonyPatch(typeof(MapSelectionGameSetting), nameof(MapSelectionGameSetting.GetValueString))]
    [HarmonyPrefix]
    public static void AddToActualOptions(MapSelectionGameSetting __instance)
    {
        if (__instance.Values.Length < 6)
        {
            __instance.Values = __instance.Values.AddItem(CustomStringNames.Submerged).ToArray();
        }
    }
}

internal static class CreateOptionsPickerPatch
{
    [HarmonyPatch]
    public static class GameOptionsMapPickerPatch
    {

        [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Start))]
        [HarmonyPrefix]
        public static void Prefix_CreateOptionsStart(CreateOptionsPicker __instance)
        {
            if (__instance.MapMenu != null)
            {
                if (!__instance.MapMenu.MapButtons.ToArray().Any(x => x.MapId == CustomMapNames.Submerged))
                {
                    SpriteRenderer subImage = new GameObject("SubmergedFilterIcon").AddComponent<SpriteRenderer>();
                    subImage.sprite = ResourceManager.spriteCache["OptionsIcon"];
                    __instance.MapMenu.MapButtons.AddItem(new MapFilterButton
                    {
                        MapId = CustomMapNames.Submerged,
                        ButtonImage = subImage
                    });

                }
            }
        }
        [HarmonyPatch(typeof(FilterMapPicker), nameof(FilterMapPicker.SetupSelectedIcons))]
        [HarmonyPrefix]
        public static void Prefix_FilterSetupIcons(FilterMapPicker __instance)
        {
            if (!__instance.mapIDs.Contains(6)) __instance.mapIDs.Add(6);
            if (!__instance.mapStrings.Contains(CustomStringNames.Submerged))
            {
                StringNames[] newMapStrings = new StringNames[__instance.mapStrings.Length + 1];
                for (int i = 0; i < __instance.mapStrings.Length; i++)
                {
                    newMapStrings[i] = __instance.mapStrings[i];
                }
                newMapStrings[__instance.mapStrings.Length] = CustomStringNames.Submerged;
                __instance.mapStrings = newMapStrings;
            }
        }
        [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Start))]
        [HarmonyPrefix]
        public static void Prefix_CGO_Start(CreateGameOptions __instance)
        {
            if (__instance.mapTooltips != null && !__instance.mapTooltips.Contains(CustomStringNames.SubmergedTooltipText))
            {
                StringNames[] newTooltips = new StringNames[__instance.mapTooltips.Length + 1];
                for (int i = 0; i < __instance.mapTooltips.Length; i++)
                {
                    newTooltips[i] = __instance.mapTooltips[i];
                }
                newTooltips[__instance.mapTooltips.Length] = CustomStringNames.SubmergedTooltipText;
                __instance.mapTooltips = newTooltips;
            }
            if (ListExtensions.TryAddItemIfNoContains(__instance.mapBanners, ResourceManager.spriteCache["OptionsLogo"], out var newArr))
            {
                __instance.mapBanners = newArr;
            }

            GameObject gameobj = GameObject.Find("MainMenuManager/MainUI/AspectScaler/CreateGameScreen/ParentContent/ConfirmPopUp");
            if (gameobj != null)
            {
                ConfirmCreatePopUp popup = gameobj.GetComponent<ConfirmCreatePopUp>();
                if (popup != null)
                {
                    if (ListExtensions.TryAddItemIfNoContains(popup.mapBanners, ResourceManager.spriteCache["OptionsLogo"], out var newArr1))
                    {
                        popup.mapBanners = newArr1;
                    }
                    if (ListExtensions.TryAddItemIfNoContains(popup.mapBanners, ResourceManager.spriteCache["OptionsBG"], out var newArr2))
                    {
                        popup.mapBanners = newArr2;
                    }
                }
            }
        }
    }
}
