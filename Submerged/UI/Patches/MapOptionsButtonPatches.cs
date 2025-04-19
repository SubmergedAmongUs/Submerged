using System.Linq;
using HarmonyLib;
using Submerged.Enums;
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
            if (__instance.MapMenu != null && __instance.MapMenu.MapButtons != null && !__instance.MapMenu.MapButtons.ToArray().Any(x => x.MapId == CustomMapNames.Submerged))
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
        [HarmonyPatch(typeof(FilterMapPicker), nameof(FilterMapPicker.SetupSelectedIcons))]
        [HarmonyPrefix]
        public static void Prefix_FilterStart(FilterMapPicker __instance)
        {
            if (__instance.mapIDs != null && !__instance.mapIDs.Contains(6)) __instance.mapIDs.Add(6);
            if (__instance.mapStrings != null && !__instance.mapStrings.Contains(CustomStringNames.Submerged))
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
        public static void Prefix_CGO_MapChanged(CreateGameOptions __instance)
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
            if (__instance.mapBanners != null && !__instance.mapBanners.Contains(ResourceManager.spriteCache["OptionsLogo"]))
            {
                Sprite[] newBanners = new Sprite[__instance.mapBanners.Length + 1];
                for (int i = 0; i < __instance.mapBanners.Length; i++)
                {
                    newBanners[i] = __instance.mapBanners[i];
                }
                newBanners[__instance.mapBanners.Length] = ResourceManager.spriteCache["OptionsLogo"];
                __instance.mapBanners = newBanners;
            }

            GameObject gameobj = GameObject.Find("MainMenuManager/MainUI/AspectScaler/CreateGameScreen/ParentContent/ConfirmPopUp");
            if (gameobj != null)
            {
                ConfirmCreatePopUp popup = gameobj.GetComponent<ConfirmCreatePopUp>();
                if (popup != null)
                {
                    if (popup.mapLogos != null && !popup.mapLogos.Contains(ResourceManager.spriteCache["OptionsLogo"]))
                    {
                        Sprite[] newLogos = new Sprite[popup.mapLogos.Length + 1];
                        for (int i = 0; i < popup.mapLogos.Length; i++)
                        {
                            newLogos[i] = popup.mapLogos[i];
                        }
                        newLogos[popup.mapLogos.Length] = ResourceManager.spriteCache["OptionsLogo"];
                        popup.mapLogos = newLogos;
                    }
                    if (popup.mapBanners != null && !popup.mapBanners.Contains(ResourceManager.spriteCache["OptionsBG"]))
                    {
                        Sprite[] newBanners = new Sprite[popup.mapBanners.Length + 1];
                        for (int i = 0; i < popup.mapBanners.Length; i++)
                        {
                            newBanners[i] = popup.mapBanners[i];
                        }
                        newBanners[popup.mapBanners.Length] = ResourceManager.spriteCache["OptionsBG"];
                        popup.mapBanners = newBanners;
                    }
                }
            }
        }
    }
}
