using System.Collections.Generic;
using Hazel;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using Submerged.Floors;
using Submerged.Map;
using Submerged.Systems.Elevator;
using AU = Submerged.BaseGame.Interfaces.AU;

namespace Submerged.Elevators;

[RegisterInIl2Cpp(typeof(ISystemType))]
public sealed class SubmarineElevatorSystem(nint ptr) : CppObject(ptr), AU.ISystemType
{
    private readonly List<PlayerControl> _inElevatorPlayers = [];
    private readonly SubmarineElevator _myElevator;

    public readonly SystemTypes systemTypes;

    public ElevatorMovementStage lastStage = ElevatorMovementStage.Complete;
    public float lerpTimer;
    public bool moving;
    public SystemTypes tandemSystemType;

    public float totalTimer;

    // This is also the floor it is currently on if not moving
    public bool upperDeckIsTargetFloor;

    public SubmarineElevatorSystem(SystemTypes systemType, bool startsOnUpper, SystemTypes tandemElevator = SystemTypes.Hallway) : this(ClassInjector.DerivedConstructorPointer<SubmarineElevatorSystem>())
    {
        ClassInjector.DerivedConstructorBody(this);

        SubmarineElevator elevator = ShipStatus.Instance.FastRooms[systemType].gameObject.GetComponent<SubmarineElevator>();
        elevator.system = this;
        SubmarineStatus.instance.elevators.Add(elevator);
        _myElevator = elevator;
        upperDeckIsTargetFloor = startsOnUpper;
        systemTypes = systemType;
        tandemSystemType = tandemElevator;
    }

    private SubmarineElevatorSystem Tandem => field ??= ShipStatus.Instance.Systems[tandemSystemType].Cast<SubmarineElevatorSystem>();

    public bool IsDirty { get; private set; }

    public void Deteriorate(float deltaTime)
    {
        if (!moving)
        {
            totalTimer = 0;
            lerpTimer = 0;
            lastStage = ElevatorMovementStage.Complete;

            return;
        }

        totalTimer += deltaTime;
        lerpTimer += deltaTime;

        if (!AmongUsClient.Instance.AmHost) return;

        ElevatorMovementStage newElevatorStage = _myElevator.GetMovementStageFromTime();

        if (lastStage != newElevatorStage)
        {
            if (newElevatorStage > ElevatorMovementStage.ElevatorMovingIn)
            {
                _myElevator.GetPlayersInElevator(_inElevatorPlayers);

                foreach (PlayerControl player in _inElevatorPlayers)
                {
                    SubmarinePlayerFloorSystem.Instance.ChangePlayerFloorState(player.PlayerId, upperDeckIsTargetFloor);
                }
            }

            lerpTimer = 0;
            IsDirty = true;
        }

        lastStage = newElevatorStage;
    }

    public void Deserialize(MessageReader reader, bool initialState)
    {
        upperDeckIsTargetFloor = reader.ReadBoolean();
        moving = reader.ReadBoolean();
        ElevatorMovementStage newLastStage = (ElevatorMovementStage) reader.ReadByte();

        if (lastStage == newLastStage) return;

        lastStage = newLastStage;
        lerpTimer = 0;
    }

    public void MarkClean()
    {
        IsDirty = false;
    }

    public void Serialize(MessageWriter writer, bool initialState)
    {
        writer.Write(upperDeckIsTargetFloor);
        writer.Write(moving);
        writer.Write((byte) lastStage);

        IsDirty = initialState;
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
        byte amount = msgReader.ReadByte();
        if (amount != 2) return;

        if (!moving)
        {
            moving = true;
            IsDirty = true;
            lerpTimer = 0;
            totalTimer = 0;
            lastStage = ElevatorMovementStage.Complete;
            upperDeckIsTargetFloor = !upperDeckIsTargetFloor;
        }

        if (tandemSystemType == SystemTypes.Hallway) return;

        SubmarineElevatorSystem tandem = Tandem;
        tandem.moving = true;
        tandem.IsDirty = true;
        tandem.lerpTimer = lerpTimer;
        tandem.totalTimer = totalTimer;
        tandem.lastStage = lastStage;
        tandem.upperDeckIsTargetFloor = moving ? !upperDeckIsTargetFloor : upperDeckIsTargetFloor;
    }
}
