using Il2CppInterop.Runtime.InteropTypes.Fields;
using Reactor.Utilities.Attributes;
using Submerged.Elevators;
using UnityEngine;
using AU = Submerged.BaseGame.Interfaces.AU;

// ReSharper disable once CheckNamespace
namespace Submerged.Systems.Elevator;

[RegisterInIl2Cpp(typeof(IUsable))]
public sealed class ElevatorConsole(nint ptr) : MonoBehaviour(ptr), AU.IUsable
{
    [UsedImplicitly]
    public Il2CppValueField<float> usableDistance; // = 0.5f

    [UsedImplicitly]
    public Il2CppReferenceField<SubmarineElevator> elevator;

    public float UsableDistance
    {
        get => usableDistance.Value;
        set => usableDistance.Value = value;
    }

    public float PercentCool => 0f;

    public ImageNames UseIcon => ImageNames.UseButton;

    public float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse)
    {
        float distance = float.MaxValue;
        PlayerControl player = pc.Object;

        couldUse = !pc.IsDead && player.CanMove && (!elevator.Value.system.moving || elevator.Value.system.lastStage == ElevatorMovementStage.Complete);
        canUse = couldUse;

        if (!canUse) return distance;
        Vector2 truePosition = player.GetTruePosition();
        Vector3 position = transform.position;
        distance = Vector2.Distance(truePosition, position);
        canUse &= distance <= UsableDistance && !PhysicsHelpers.AnythingBetween(truePosition, position, Constants.ShipOnlyMask, false);

        return distance;
    }

    public void SetOutline(bool on, bool mainTarget) { }

    public void Use()
    {
        CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);

        if (!canUse) return;
        elevator.Value.Use();
    }
}
