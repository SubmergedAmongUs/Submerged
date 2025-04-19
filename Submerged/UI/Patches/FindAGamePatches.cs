using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
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
        if (__instance.mapLogoSprites != null && !__instance.mapLogoSprites.Contains(ResourceManager.spriteCache["OptionsLogo"]))
        {
            Sprite[] newLogos = new Sprite[__instance.mapLogoSprites.Length + 1];
            for (int i = 0; i < __instance.mapLogoSprites.Length; i++)
            {
                newLogos[i] = __instance.mapLogoSprites[i];
            }
            newLogos[__instance.mapLogoSprites.Length] = ResourceManager.spriteCache["OptionsLogo"];
            __instance.mapLogoSprites = newLogos;
        }
        if (__instance.mapBackgroundSprites != null && !__instance.mapBackgroundSprites.Contains(ResourceManager.spriteCache["OptionsBG"]))
        {
            Sprite[] newBanners = new Sprite[__instance.mapBackgroundSprites.Length + 1];
            for (int i = 0; i < __instance.mapBackgroundSprites.Length; i++)
            {
                newBanners[i] = __instance.mapBackgroundSprites[i];
            }
            newBanners[__instance.mapBackgroundSprites.Length] = ResourceManager.spriteCache["OptionsBG"];
            __instance.mapBackgroundSprites = newBanners;
        }
    }
}
