using System.Linq;
using HarmonyLib;
using Submerged.Extensions;
using Submerged.Map;

namespace Submerged.Elevators.Patches;

[HarmonyPatch]
public static class PreventPettingInElevatorPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanPet))]
    [HarmonyPostfix]
    public static void CanPetPatch(ref bool __result)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        __result &= !SubmarineStatus.instance.elevators.Any(elevator => elevator.LocalPlayerInElevator);
    }
}
