using HarmonyLib;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Map.Patches;

[HarmonyPatch]
public static class DecontaminationLightsPatches
{
    [HarmonyPatch(typeof(DeconSystem), nameof(DeconSystem.UpdateDoorsViaState))]
    [HarmonyPrefix]
    public static void UpdateLightsBasedOnSystemStatePatch(DeconSystem __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        Transform light = __instance.transform.Find($"{__instance.name}Light/LightOn");

        if (!light) return;
        light.gameObject.SetActive(!__instance.CurState.HasFlag(DeconSystem.States.Closed));
    }
}
