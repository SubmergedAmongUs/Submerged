using System.Linq;
using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.Floors;
using Submerged.Map;
using Submerged.Systems.Elevator;
using UnityEngine;

namespace Submerged.Elevators.Objects;

[RegisterInIl2Cpp]
public sealed class MultifloorArrowBehaviour(nint ptr) : MonoBehaviour(ptr)
{
    private static SCG.List<(SubmarineElevator elevator, Vector3 position)> _lowerElevatorPositions;
    private static SCG.List<(SubmarineElevator elevator, Vector3 position)> _upperElevatorPositions;

    public ArrowBehaviour arrowBehaviour;

    public Vector3 lastSetTarget;
    public Vector3 originalTarget;

    private void Awake()
    {
        arrowBehaviour = GetComponent<ArrowBehaviour>();
    }

    public void Update()
    {
        if (lastSetTarget != arrowBehaviour.target)
        {
            originalTarget = arrowBehaviour.target;
        }

        lastSetTarget = arrowBehaviour.target = GetElevatorPosition(originalTarget);
    }

    public static Vector3 GetElevatorPosition(Vector3 target)
    {
        if (_lowerElevatorPositions == null || _upperElevatorPositions == null || !_lowerElevatorPositions[0].elevator)
        {
            _lowerElevatorPositions = SubmarineStatus.instance.elevators.Select(e => (e, e.lowerOuterDoor.Value.transform.position)).ToList();
            _upperElevatorPositions = SubmarineStatus.instance.elevators.Select(e => (e, e.upperOuterDoor.Value.transform.position)).ToList();
        }

        FloorHandler floorHandler = FloorHandler.LocalPlayer;

        Vector3 localPlayerPos = PlayerControl.LocalPlayer.transform.position;

        if (target.y < FloorHandler.FLOOR_CUTOFF) // If target is on lower deck
        {
            if (!floorHandler.onUpper) return target;
        }
        else
        {
            if (floorHandler.onUpper) return target;
        }

        float maxDist = float.MaxValue;
        Vector2 current = Vector2.zero;
        int elevatorIndex = 0;

        for (int index = 0; index < (floorHandler.onUpper ? _upperElevatorPositions : _lowerElevatorPositions).Count; index++)
        {
            (SubmarineElevator elevator, Vector3 pos) = (floorHandler.onUpper ? _upperElevatorPositions : _lowerElevatorPositions)[index];
            if (elevator.system.systemTypes != CustomSystemTypes.ElevatorService &&
                elevator.system.upperDeckIsTargetFloor != floorHandler.onUpper) continue;

            float distance = Vector2.Distance(pos, localPlayerPos);

            if (distance < maxDist)
            {
                maxDist = distance;
                current = pos;
                elevatorIndex = index;
            }
        }

        if (SubmarineStatus.instance.elevators[elevatorIndex].CheckInElevator(localPlayerPos)) current = localPlayerPos;

        return current;
    }
}
