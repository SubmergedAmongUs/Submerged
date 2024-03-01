using AmongUs.Data;
using HarmonyLib;
using Submerged.Extensions;
using Submerged.Map;
using TMPro;
using UnityEngine;

namespace Submerged.Elevators.Patches;

[HarmonyPatch]
public static class ForceElevatorScreenShakePatches
{
    [HarmonyPatch(typeof(TabGroup), nameof(TabGroup.Open))]
    [HarmonyPostfix]
    public static void DisableScreenshakeButtonPatch(TabGroup __instance)
    {
        if (!SubmarineStatus.instance || __instance.name != "GraphicsButton") return;

        GameObject setting = __instance.Content.transform.Find("Screenshake").gameObject;
        setting.GetComponent<ButtonRolloverHandler>().enabled = false;
        setting.GetComponent<PassiveButton>().enabled = false;
        setting.GetComponent<ToggleButtonBehaviour>().enabled = false;

        setting.GetComponentInChildren<SpriteRenderer>().color = Palette.DisabledGrey;
        setting.GetComponentInChildren<TextMeshPro>().color = Palette.DisabledGrey;
    }

    [HarmonyPatch(typeof(ResolutionSlider), nameof(ResolutionSlider.OnEnable))]
    public static class DisableScreenShakeSettingPatch
    {
        [HarmonyPrefix, UsedImplicitly]
        public static void Prefix(ref bool __state)
        {
            if (!ShipStatus.Instance.IsSubmerged()) return;

            __state = DataManager.Settings.Gameplay.screenShake;
            DataManager.Settings.Gameplay.screenShake = true;
        }

        [HarmonyPostfix, UsedImplicitly]
        public static void Postfix(bool __state)
        {
            if (!ShipStatus.Instance.IsSubmerged()) return;

            DataManager.Settings.Gameplay.screenShake = __state;
        }
    }

    [HarmonyPatch(typeof(FollowerCamera), nameof(FollowerCamera.Update))]
    public static class AlwaysShakeScreenPatch
    {
        [HarmonyPrefix, UsedImplicitly]
        public static void Prefix(ref bool __state)
        {
            if (!ShipStatus.Instance.IsSubmerged()) return;

            __state = DataManager.Settings.Gameplay.screenShake;
            DataManager.Settings.Gameplay.screenShake = true;
        }

        [HarmonyPostfix, UsedImplicitly]
        public static void Postfix(bool __state)
        {
            if (!ShipStatus.Instance.IsSubmerged()) return;

            DataManager.Settings.Gameplay.screenShake = __state;
        }
    }
}
