using HarmonyLib;

namespace Submerged.Debugging.Patches;

[DebugHarmonyPatch]
public static class GameTestingPatches
{
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    [HarmonyPrefix]
    public static void AllowStartingWithOnePlayerPatch(GameStartManager __instance)
    {
        __instance.MinPlayers = 1;
    }

    [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
    [HarmonyPostfix]
    public static void PreventBanPatch(out bool __result)
    {
        __result = false;
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    [HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.CheckEndCriteria))]
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.IsGameOverDueToDeath))]
    [HarmonyPatch(typeof(LogicGameFlowHnS), nameof(LogicGameFlowHnS.IsGameOverDueToDeath))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool StopGameEndingPatch() => false;

    [HarmonyPatch(typeof(AprilFoolsMode), nameof(AprilFoolsMode.ShouldShowAprilFoolsToggle))]
    [HarmonyPrefix]
    public static bool EnableAprilFoolsTogglePatch(out bool __result)
    {
        __result = true;
        return false;
    }
}
