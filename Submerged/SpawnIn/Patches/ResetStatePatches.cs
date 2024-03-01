using HarmonyLib;
using Submerged.Extensions;
using Submerged.SpawnIn.Enums;

namespace Submerged.SpawnIn.Patches;

[HarmonyPatch]
public static class ResetStatePatches
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    [HarmonyPostfix]
    public static void ResetStateWhenMeetingCalledPatch()
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        SubmarineSpawnInSystem.Instance.currentState = SpawnInState.Loading;
        SubmarineSpawnInSystem.Instance.players.Clear();
        SubmarineSpawnInSystem.Instance.timer = 10;
        SubmarineSpawnInSystem.Instance.IsDirty = true;
    }
}
