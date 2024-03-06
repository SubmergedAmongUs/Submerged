using HarmonyLib;
using Submerged.Extensions;
using static Submerged.Vents.VentPatchData;

namespace Submerged.Vents.Patches;

[HarmonyPatch]
public static class HideAndSeekPatches
{
    [HarmonyPatch(typeof(LogicUsablesHnS), nameof(LogicUsablesHnS.CanUse))]
    [HarmonyPostfix]
    public static void DisableCrossFloorVentsPatch(ref bool __result, [HarmonyArgument(0)] IUsable usable, [HarmonyArgument(1)] PlayerControl player)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        if (usable.TryCast<Vent>() is not { } vent) return;

        __result = __result && vent.Id is not (LOWER_CENTRAL_VENT_ID or UPPER_CENTRAL_VENT_ID or ADMIN_VENT_ID);
    }
}
