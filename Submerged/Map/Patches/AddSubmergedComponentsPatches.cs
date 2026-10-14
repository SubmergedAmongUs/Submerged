using System;
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
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void SupplyCurrentGamePrefabsPatch(ShipStatus __instance)
    {
        if (!__instance.IsSubmerged() || __instance.socialMediumFeedSystemPrefab) return;

        // The existing asset bundle predates the Social Medium system. ShipStatus.Awake
        // needs its prefab before SubmarineStatus can initialize the map.
        ShipStatus source = MapLoader.Skeld;
        if (!source || !source.socialMediumFeedSystemPrefab) source = MapLoader.Airship;
        if (!source || !source.socialMediumFeedSystemPrefab) source = MapLoader.Fungle;
        if (!source || !source.socialMediumFeedSystemPrefab)
        {
            throw new InvalidOperationException("The vanilla Social Medium prefab must be loaded before Submerged awakens.");
        }

        __instance.socialMediumFeedSystemPrefab = source.socialMediumFeedSystemPrefab;
    }

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
