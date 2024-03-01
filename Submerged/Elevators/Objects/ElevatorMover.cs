using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.Floors;
using Submerged.Map;
using Submerged.Systems.Elevator;
using UnityEngine;

namespace Submerged.Elevators.Objects;

[RegisterInIl2Cpp]
public class ElevatorMover(nint ptr) : MonoBehaviour(ptr)
{
    private void Update()
    {
        if (!ShipStatus.Instance.IsSubmerged())
        {
            enabled = false;

            return;
        }

        SubmarineElevator currentElevator = null;

        foreach (SubmarineElevator submarineElevator in SubmarineStatus.instance.elevators)
        {
            if (submarineElevator.CheckInElevator(transform.position)) currentElevator = submarineElevator;
        }

        if (!currentElevator) return;

        if (!currentElevator.system.moving)
        {
            MoveFloor(currentElevator.system.upperDeckIsTargetFloor);

            return;
        }

        ElevatorMovementStage stage = currentElevator.GetMovementStageFromTime();

        if (!PlayerControl.LocalPlayer.Data.IsDead && currentElevator.GetInElevator(PlayerControl.LocalPlayer))
        {
            MoveFloor(PlayerControl.LocalPlayer.transform.position.y > FloorHandler.FLOOR_CUTOFF);

            return;
        }

        MoveFloor(stage >= ElevatorMovementStage.FadingToClear ? currentElevator.system.upperDeckIsTargetFloor : !currentElevator.system.upperDeckIsTargetFloor);
    }

    public void MoveFloor(bool onUpper)
    {
        Vector3 position = transform.position;
        bool currentlyOnUpper = position.y > FloorHandler.FLOOR_CUTOFF;

        if (onUpper == currentlyOnUpper) return;

        position.y += FloorHandler.MAP_OFFSET * (onUpper ? 1f : -1f);
        position.z = position.y / 1000f;
        transform.position = position;
    }
}
