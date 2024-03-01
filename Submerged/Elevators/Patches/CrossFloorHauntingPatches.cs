using HarmonyLib;
using Submerged.Extensions;
using Submerged.Floors;

namespace Submerged.Elevators.Patches;

[HarmonyPatch]
public static class CrossFloorHauntingPatches
{
    [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.FixedUpdate))]
    [HarmonyPostfix]
    public static void SwitchToTargetFloorPatch(HauntMenuMinigame __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;
        if (!__instance.HauntTarget) return;

        FloorHandler myFloorHandler = FloorHandler.LocalPlayer;
        FloorHandler targetFloorHandler = FloorHandler.GetFloorHandler(__instance.HauntTarget.MyPhysics);

        if (myFloorHandler.onUpper != targetFloorHandler.onUpper)
        {
            myFloorHandler.RpcRequestChangeFloor(targetFloorHandler.onUpper);
        }
    }
}
