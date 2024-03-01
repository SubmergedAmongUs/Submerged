using BepInEx.Unity.IL2CPP.Utils;
using HarmonyLib;
using Submerged.Extensions;
using Submerged.Floors;
using UnityEngine;
using static Submerged.Vents.VentPatchData;

namespace Submerged.Vents.Patches;

[HarmonyPatch]
public static class MovementPatches
{
    private static bool _enableRpcSnapToPatch;

    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.RpcSnapTo))]
    [HarmonyPrefix]
    public static bool RpcSnapToPatch([HarmonyArgument(0)] Vector2 position)
    {
        if (!_enableRpcSnapToPatch) return true;
        DisablePatch();

        if (!ShipStatus.Instance.IsSubmerged()) return true;

        Vent current = Vent.currentVent;
        Vent target = GetClosestVent(position);

        float currentY = current.transform.position.y;
        float targetY = target.transform.position.y;

        if (target.Id == ENGINE_ROOM_VENT_ID)
        {
            PlayerControl.LocalPlayer.StartCoroutine(EngineVentMovement.HandleMove(position));
            return false;
        }

        if (target.Id is UPPER_CENTRAL_VENT_ID or LOWER_CENTRAL_VENT_ID)
        {
            PlayerControl.LocalPlayer.StartCoroutine(CrossFloorVentMovement.HandleCentralMove(position));
            return false;
        }

        if (currentY < FloorHandler.FLOOR_CUTOFF != targetY < FloorHandler.FLOOR_CUTOFF)
        {
            PlayerControl.LocalPlayer.StartCoroutine(CrossFloorVentMovement.HandleMove(position));
            return false;
        }

        return true;
    }

    private static Vent GetClosestVent(Vector2 position)
    {
        (Vent item, float distance) closest = (null, float.MaxValue);

        foreach (Vent vent in ShipStatus.Instance.AllVents)
        {
            float distance = Vector2.Distance(position, vent.transform.position);

            if (distance < closest.distance)
            {
                closest = (vent, distance);
            }
        }

        return closest.item;
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.TryMoveToVent))]
    [HarmonyPrefix]
    public static void EnablePatch()
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        _enableRpcSnapToPatch = true;
    }

    [HarmonyPatch(typeof(Vent), nameof(Vent.TryMoveToVent))]
    [HarmonyPostfix]
    public static void DisablePatch()
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        _enableRpcSnapToPatch = false;
    }
}
