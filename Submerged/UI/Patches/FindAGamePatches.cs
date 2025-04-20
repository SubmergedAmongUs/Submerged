﻿using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using Submerged.Extensions;
using Submerged.Resources;
using UnityEngine;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class FindAGamePatches
{
    [HarmonyPatch(typeof(MatchMakerGameButton), nameof(MatchMakerGameButton.SetGame))]
    [HarmonyPrefix]
    public static void AddSubmergedIconPatch(MatchMakerGameButton __instance, GameListing gameListing)
    {
        if (__instance.MapIcons.Length < 5) __instance.MapIcons = UpdateMapIcons(__instance.MapIcons).ToArray();
    }

    private static SCG.IEnumerable<Sprite> UpdateMapIcons(Il2CppReferenceArray<Sprite> existingIcons)
    {
        yield return existingIcons[0]; // Skeld
        yield return existingIcons[1]; // MIRA HQ
        yield return existingIcons[2]; // Polus
        yield return existingIcons[0]; // Dleks
        yield return existingIcons[3]; // Airship
        yield return existingIcons[4]; // Fungle
        yield return ResourceManager.spriteCache["FilterIcon"];
    }
    [HarmonyPatch(typeof(GameContainer), nameof(GameContainer.SetupGameInfo))]
    [HarmonyPrefix]
    public static void AddSubmergedIconAndBG(GameContainer __instance)
    {
        if (ListExtensions.TryAddItemIfNoContains(__instance.mapLogoSprites, ResourceManager.spriteCache["OptionsLogo"], out var newArr))
        {
            __instance.mapLogoSprites = newArr;
        }
        if (ListExtensions.TryAddItemIfNoContains(__instance.mapBackgroundSprites, ResourceManager.spriteCache["OptionsBG"], out var newArr1))
        {
            __instance.mapBackgroundSprites = newArr1;
        }
    }
}
