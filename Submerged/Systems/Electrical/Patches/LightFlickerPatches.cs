using HarmonyLib;
using Submerged.Extensions;
using Submerged.Map;

namespace Submerged.Systems.Electrical.Patches;

[HarmonyPatch]
public static class LightFlickerPatches
{
    private static bool IsLightFlickerActive => ShipStatus.Instance.IsSubmerged() && SubmarineStatus.instance.lightFlickerActive;

    [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool CantClickDeadBodyPatch() => !IsLightFlickerActive;

    [HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.ValidConsole))]
    [HarmonyPostfix]
    public static void CantFixLightsPatch(ref bool __result)
    {
        if (IsLightFlickerActive) __result = false;
    }

    [HarmonyPatch(typeof(ElectricTask), nameof(ElectricTask.FixedUpdate))]
    [HarmonyPostfix]
    public static void DontShowFixLightsPatch(ElectricTask __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        if (__instance.IsComplete || IsLightFlickerActive) return;

        __instance.SetupArrows();
        __instance.HasLocation = true;
    }

    [HarmonyPatch(typeof(ReportButton), nameof(ReportButton.DoClick))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool CantClickReportButtonPatch() => !IsLightFlickerActive;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void DontShowReportButtonPatch()
    {
        if (!IsLightFlickerActive) return;
        HudManager.Instance.ReportButton.SetActive(false);
    }
}
