using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Loading;
using Submerged.Resources;
using UnityEngine;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class MapSelectButtonPatches
{
    private static FreeplayPopover _lastInstance;

    [HarmonyPatch(typeof(IGameOptionsExtensions), nameof(IGameOptionsExtensions.ToggleMapFilter))]
    [HarmonyPrefix]
    public static bool AllowSelectingSubmergedMapPatch([HarmonyArgument(0)] IGameOptions gameOptions, [HarmonyArgument(1)] byte newId)
    {
        int b = gameOptions.MapId ^ (byte) (1 << newId);
        if (b != 0) gameOptions.SetByte(ByteOptionNames.MapId, (byte) b);

        return false;
    }

    [HarmonyPatch(typeof(FreeplayPopover), nameof(FreeplayPopover.Show))]
    [HarmonyPrefix]
    public static void AdjustFreeplayMenuPatch(FreeplayPopover __instance)
    {
        if (_lastInstance == __instance || AssetLoader.Errored) return;
        _lastInstance = __instance;

        FreeplayPopoverButton fungleButton = __instance.buttons[4];
        FreeplayPopoverButton submergedButton = UnityObject.Instantiate(fungleButton, fungleButton.transform.parent);

        submergedButton.name = "SubmergedButton";
        submergedButton.map = CustomMapNames.Submerged;
        submergedButton.GetComponent<SpriteRenderer>().sprite = ResourceManager.spriteCache["Logo"];
        submergedButton.OnPressEvent = fungleButton.OnPressEvent;

        fungleButton.transform.position = new Vector3(__instance.buttons[0].transform.position.x, fungleButton.transform.position.y, fungleButton.transform.position.z);
        submergedButton.transform.position = new Vector3(__instance.buttons[1].transform.position.x, fungleButton.transform.position.y, fungleButton.transform.position.z);

        SwapPositionsTroll(fungleButton, submergedButton);

        __instance.buttons = new List<FreeplayPopoverButton>(__instance.buttons) { submergedButton }.ToArray();
    }

    [HarmonyPatch(typeof(CreateGameOptions), nameof(CreateGameOptions.Start))]
    [HarmonyPrefix]
    public static void AddSubmergedToCreateGame(CreateGameOptions __instance)
    {
        __instance.mapTooltips = __instance.mapTooltips.AddItem(CustomStringNames.SubmergedTooltip).ToArray();
        __instance.mapBanners = new List<Sprite>(__instance.mapBanners)
        {
            ResourceManager.spriteCache["OptionsLogo"]
        }.ToArray();

        ConfirmCreatePopUp popUp = __instance.confirmPopUp.GetComponent<ConfirmCreatePopUp>();

        popUp.mapLogos = new List<Sprite>(popUp.mapLogos)
        {
            ResourceManager.spriteCache["OptionsLogo"]
        }.ToArray();

        popUp.mapBanners = new List<Sprite>(popUp.mapBanners)
        {
            ResourceManager.spriteCache["OptionsBG"]
        }.ToArray();
    }

    private static void SwapPositionsTroll(Component one, Component two)
    {
        (one.transform.position, two.transform.position) = (two.transform.position, one.transform.position);
    }
}
