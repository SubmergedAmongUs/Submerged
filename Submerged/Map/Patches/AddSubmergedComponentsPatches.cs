using System.Linq;
using HarmonyLib;
using Submerged.Elevators.Objects;
using Submerged.Extensions;
using Submerged.Floors.Objects;

namespace Submerged.Map.Patches;

[HarmonyPatch]
public static class AddSubmergedComponentsPatches
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    [HarmonyPostfix]
    public static void AddShipStatusComponentPatch(ShipStatus __instance)
    {
        if (!__instance.IsSubmerged()) return;

        __instance.gameObject.AddComponent<SubmarineStatus>();
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    [HarmonyPostfix]
    public static void AddDeadBodyComponentPatch(PlayerControl __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        DeadBody body = UnityObject.FindObjectsOfType<DeadBody>()?.FirstOrDefault(b => b.ParentId == __instance.PlayerId);

        if (body)
        {
            body.gameObject.AddComponent<GenericShadowBehaviour>();
            body.gameObject.AddComponent<ElevatorMover>();
        }
    }
}
