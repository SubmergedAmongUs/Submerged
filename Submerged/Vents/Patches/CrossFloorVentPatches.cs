using HarmonyLib;
using Submerged.Extensions;
using Submerged.Floors;
using UnityEngine;
using static Submerged.Vents.VentPatchData;

namespace Submerged.Vents.Patches;

[HarmonyPatch]
public static class CrossFloorVentPatches
{
    [HarmonyPatch(typeof(Vent), nameof(Vent.SetButtons))]
    [HarmonyPostfix]
    public static void ColorVentMoveArrowsPatch(Vent __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        for (int i = 0; i < __instance.NearbyVents.Length; i++)
        {
            Vent vent = __instance.NearbyVents[i];
            if (!vent || vent.Id == ENGINE_ROOM_VENT_ID) continue;

            SpriteRenderer button = __instance.Buttons[i].GetComponent<SpriteRenderer>();

            button.color = __instance.transform.position.y < FloorHandler.FLOOR_CUTOFF != vent.transform.position.y < FloorHandler.FLOOR_CUTOFF
                ? Color.yellow
                : Color.white;

            __instance.UpdateArrows(ShipStatus.Instance.Systems[SystemTypes.Ventilation].Cast<VentilationSystem>());
        }
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void PreventMovingDuringTransitionPatch(ref float __result, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        if (InTransition)
        {
            __result = float.MaxValue;
            canUse = couldUse = false;
        }
    }
}
