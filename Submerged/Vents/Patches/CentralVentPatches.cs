using HarmonyLib;
using Submerged.Extensions;
using UnityEngine;
using static Submerged.Vents.VentPatchData;
using PlayerPhysics_CoExitVent = PlayerPhysics._CoExitVent_d__48;

namespace Submerged.Vents.Patches;

[HarmonyPatch]
public static class CentralVentPatches
{
    [HarmonyPatch]
    public static class AnythingBetweenPatch
    {
        private static bool _enableAnythingBetweenPatch;

        [HarmonyPatch(typeof(PhysicsHelpers), nameof(PhysicsHelpers.AnythingBetween), typeof(Collider2D), typeof(Vector2), typeof(Vector2), typeof(int), typeof(bool))]
        [HarmonyPrefix]
        public static bool AnythingBetweenBypassPatch() => !_enableAnythingBetweenPatch;

        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static void EnablePatch(Vent __instance)
        {
            if (!ShipStatus.Instance.IsSubmerged()) return;
            if (__instance.Id != LOWER_CENTRAL_VENT_ID) return;

            _enableAnythingBetweenPatch = true;
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void DisablePatch()
        {
            _enableAnythingBetweenPatch = false;
        }
    }

    [HarmonyPatch]
    public static class StopMoveUpPatch
    {
        private static bool _enableSnapToPatch;

        [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.SnapTo), typeof(Vector2))]
        [HarmonyPrefix, HarmonyPriority(Priority.First + 100)]
        public static void Prefix([HarmonyArgument(0)] ref Vector2 vector)
        {
            if (!_enableSnapToPatch) return;
            DisablePatch();

            if (!ShipStatus.Instance.IsSubmerged()) return;

            vector -= new Vector2(0, 0.2f);
        }

        [HarmonyPatch(typeof(PlayerPhysics_CoExitVent), nameof(PlayerPhysics_CoExitVent.MoveNext))]
        [HarmonyPrefix]
        public static void EnablePatch(PlayerPhysics_CoExitVent __instance)
        {
            if (!ShipStatus.Instance.IsSubmerged()) return;
            if (__instance.__1__state != 0) return;

            _enableSnapToPatch = true;
        }

        [HarmonyPatch(typeof(PlayerPhysics_CoExitVent), nameof(PlayerPhysics_CoExitVent.MoveNext))]
        [HarmonyPostfix]
        public static void DisablePatch()
        {
            _enableSnapToPatch = false;
        }
    }
}
