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

    [HarmonyPatch(typeof(CreateOptionsPicker), nameof(CreateOptionsPicker.Awake))]
    [HarmonyPrefix]
    public static void AdjustCreateOptionsPatch(CreateOptionsPicker __instance)
    {
        Transform mapPicker = __instance.transform.Find("MapPicker");
        MapPickerMenu mapPickerMenu = mapPicker.Find("Map Picker Menu").GetComponent<MapPickerMenu>();

        if (__instance.mode == SettingsMode.Host)
        {
            mapPickerMenu.transform.localPosition = new Vector3(mapPickerMenu.transform.localPosition.x, 1, mapPickerMenu.transform.localPosition.z);
        }

        Transform fungleButton = mapPickerMenu.transform.Find("Fungle");
        Transform submergedButton = UnityObject.Instantiate(fungleButton, fungleButton.parent);
        submergedButton.name = "Submerged";
        submergedButton.position = new Vector3(fungleButton.position.x,
            2 * fungleButton.position.y - mapPickerMenu.transform.Find("Airship").transform.position.y,
            fungleButton.position.z);

        SpriteRenderer submergedButtonRenderer = submergedButton.Find("Image").GetComponent<SpriteRenderer>();
        submergedButtonRenderer.sprite = ResourceManager.spriteCache["Logo"];

        PassiveButton submergedPassiveButton = submergedButton.GetComponent<PassiveButton>();
        submergedPassiveButton.OnClick.m_PersistentCalls.m_Calls._items[0].arguments.intArgument = (int) CustomMapTypes.Submerged;

        MapFilterButton fungleIndicator = __instance.MapMenu.MapButtons[4];
        MapFilterButton submergedIndicator = UnityObject.Instantiate(fungleIndicator, fungleIndicator.transform.parent);
        submergedIndicator.name = "Submerged";
        submergedIndicator.transform.localPosition = new Vector3(0.80f, fungleIndicator.transform.localPosition.y, fungleIndicator.transform.localPosition.z);
        submergedIndicator.MapId = CustomMapNames.Submerged;
        submergedIndicator.Button = submergedPassiveButton;
        submergedIndicator.ButtonCheck = submergedButton.Find("selectedCheck").GetComponent<SpriteRenderer>();
        submergedIndicator.ButtonImage = submergedButtonRenderer;
        submergedIndicator.ButtonOutline = submergedButtonRenderer.transform.parent.GetComponent<SpriteRenderer>();
        submergedIndicator.Icon.sprite = ResourceManager.spriteCache["FilterIcon"];
        __instance.MapMenu.MapButtons = __instance.MapMenu.MapButtons.AddItem(submergedIndicator).ToArray();

        float pos = -1;
        for (int i = 0; i < 6; i++)
        {
            __instance.MapMenu.MapButtons[i].transform.SetLocalX(pos);
            pos += 0.34f;
        }

        SwapPositionsTroll(fungleButton, submergedButton);
        SwapPositionsTroll(fungleIndicator, submergedIndicator);

        if (AssetLoader.Errored)
        {
            submergedPassiveButton.enabled = false;
            submergedButtonRenderer.color = Palette.DisabledGrey;

            IGameOptions targetOptions = __instance.GetTargetOptions();

            if (targetOptions.MapId == (byte) CustomMapTypes.Submerged)
            {
                targetOptions.SetByte(ByteOptionNames.MapId, 0);
                __instance.SetTargetOptions(targetOptions);
                __instance.MapMenu.UpdateMapButtons(0);
            }
        }

        if (__instance.mode == SettingsMode.Host)
        {
            __instance.CrewArea.MapBackgrounds = new List<Sprite>(__instance.CrewArea.MapBackgrounds) { ResourceManager.spriteCache["CreateGameBG"] }.ToArray();

            mapPicker.localScale = new Vector3(0.75f, 0.75f, 1);
        }

        mapPickerMenu.transform.Find("Backdrop").localScale *= 4;
    }

    private static void SwapPositionsTroll(Component one, Component two)
    {
        (one.transform.position, two.transform.position) = (two.transform.position, one.transform.position);
    }
}
