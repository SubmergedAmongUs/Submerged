using System.Linq;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Minigames.CustomMinigames.RetrieveOxygenMask;

namespace Submerged.Systems.Oxygen.Patches;

[HarmonyPatch]
public static class TaskPatches
{
    [HarmonyPatch]
    public static class TaskIsEmergencyPatch
    {
        private static bool _overrideFalse = false;

        [HarmonyPatch(typeof(PlayerTask), nameof(PlayerTask.TaskIsEmergency))]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void AllowButtonsFromOtherPlayersDuringO2Patch([HarmonyArgument(0)] PlayerTask arg, ref bool __result)
        {
            if (!ShipStatus.Instance.IsSubmerged()) return;

            // If trying to call a meeting then allow it
            // Even though we're ignoring a bunch of checks here, they were already performed when the meeting was initially called.
            // I think this was here to allow other clients to call the button (we can't check that condition here)
            if (_overrideFalse) __result = false;
        }

        [HarmonyPatch(typeof(PlayerTask), nameof(PlayerTask.TaskIsEmergency))]
        [HarmonyPostfix]
        public static void AllowButtonAfterGrabbingMaskPatch([HarmonyArgument(0)] PlayerTask arg, ref bool __result)
        {
            if (arg.TryCast<OxygenSabotageTask>() is not { } o2) return;

            __result = !o2.system.playersWithMask.Contains(PlayerControl.LocalPlayer.PlayerId);
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
        [HarmonyPrefix]
        public static void DisablePatch()
        {
            _overrideFalse = true;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
        [HarmonyPostfix]
        public static void EnablePatch()
        {
            _overrideFalse = false;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.GetSabotageTask))]
    [HarmonyPrefix]
    public static bool Prefix(ShipStatus __instance, ref PlayerTask __result, [HarmonyArgument(0)] SystemTypes system)
    {
        if (!__instance.IsSubmerged()) return true;

        if (system == SystemTypes.LifeSupp)
        {
            __result = __instance.SpecialTasks.First(t => t.TaskType == CustomTaskTypes.RetrieveOxygenMask);
            return false;
        }

        return true;
    }
}
