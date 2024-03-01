using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Floors;
using Submerged.Map;
using Submerged.Systems.Elevator;
using UnityEngine;

namespace Submerged.Minigames.Patches;

[HarmonyPatch(typeof(ArrowBehaviour), nameof(ArrowBehaviour.UpdatePosition))]
public static class ArrowBehaviourUpdatePositionPatch
{
    private static List<(SubmarineElevator elevator, Vector3 position)> _lowerElevatorPositions;
    private static List<(SubmarineElevator elevator, Vector3 position)> _upperElevatorPositions;

    public static Vector3 GetElevatorPosition(ArrowBehaviour __instance)
    {
        if (_lowerElevatorPositions == null || _upperElevatorPositions == null || !_lowerElevatorPositions[0].elevator)
        {
            _lowerElevatorPositions = SubmarineStatus.instance.elevators.Select(e => (e, e.lowerOuterDoor.Value.transform.position)).ToList();
            _upperElevatorPositions = SubmarineStatus.instance.elevators.Select(e => (e, e.upperOuterDoor.Value.transform.position)).ToList();
        }

        FloorHandler floorHandler = FloorHandler.LocalPlayer;

        Vector3 position = PlayerControl.LocalPlayer.transform.position;

        if (__instance.target.y < FloorHandler.FLOOR_CUTOFF)
        {
            if (!floorHandler.onUpper) return __instance.target;
        }
        else
        {
            if (floorHandler.onUpper) return __instance.target;
        }

        float maxDist = float.MaxValue;
        Vector2 current = Vector2.zero;
        int elevatorIndex = 0;

        for (int index = 0; index < (floorHandler.onUpper ? _upperElevatorPositions : _lowerElevatorPositions).Count; index++)
        {
            (SubmarineElevator elevator, Vector3 pos) = (floorHandler.onUpper ? _upperElevatorPositions : _lowerElevatorPositions)[index];
            if (elevator.system.systemTypes != CustomSystemTypes.ElevatorService &&
                elevator.system.upperDeckIsTargetFloor != floorHandler.onUpper) continue;

            float distance = Vector2.Distance(pos, position);

            if (distance < maxDist)
            {
                maxDist = distance;
                current = pos;
                elevatorIndex = index;
            }
        }

        if (SubmarineStatus.instance.elevators[elevatorIndex].CheckInElevator(position)) current = position;

        return current;
    }

    [HarmonyPrefix]
    public static bool Prefix(ArrowBehaviour __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return true;

        Camera main = Camera.main!;
        Vector3 newTarget = GetElevatorPosition(__instance);
        Vector2 vector = newTarget - main.transform.position;
        float num = vector.magnitude / (main.orthographicSize * __instance.perc);
        __instance.image.enabled = num > 0.3;
        Vector2 vector2 = main.WorldToViewportPoint(newTarget);

        if (__instance.Between(vector2.x, 0f, 1f) && __instance.Between(vector2.y, 0f, 1f))
        {
            __instance.transform.position = newTarget - (Vector3) vector.normalized * 0.6f;
            float num2 = Mathf.Clamp(num, 0f, 1f);
            __instance.transform.localScale = new Vector3(num2, num2, num2);
        }
        else
        {
            Vector2 vector3 = new Vector3(Mathf.Clamp(vector2.x * 2f - 1f, -1f, 1f), Mathf.Clamp(vector2.y * 2f - 1f, -1f, 1f));
            float orthographicSize = main.orthographicSize;
            float num3 = main.orthographicSize * main.aspect;
            Vector3 vector4 = new(Mathf.LerpUnclamped(0f, num3 * 0.88f, vector3.x), Mathf.LerpUnclamped(0f, orthographicSize * 0.79f, vector3.y), 0f);
            __instance.transform.position = main.transform.position + vector4;
            __instance.transform.localScale = Vector3.one;
        }

        __instance.transform.LookAt2d(newTarget);

        return false;
    }
}
