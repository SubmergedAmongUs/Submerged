using System;
using System.Collections.Generic;
using System.Linq;

namespace Submerged.Enums;

public readonly struct CustomSystemTypes : IEquatable<CustomSystemTypes>
{
    internal static void Initialize()
    {
        SystemTypeHelpers.AllTypes = SystemTypeHelpers.AllTypes.Concat(All.Select(t => t.systemType)).ToArray();
    }

    #region Struct implementation

    public static List<CustomSystemTypes> All { get; } = [];
    private static readonly Dictionary<SystemTypes, CustomSystemTypes> _mapping = new();

    public readonly SystemTypes systemType;
    public readonly StringNames stringName;

    private CustomSystemTypes(int systemType, StringNames stringName = StringNames.None)
    {
        this.systemType = (SystemTypes) systemType;
        this.stringName = stringName;

        All.Add(this);
        _mapping.Add(this.systemType, this);
    }

    public static bool TryGetFromSystemType(SystemTypes systemTypes, out CustomSystemTypes result) => _mapping.TryGetValue(systemTypes, out result);

    public static implicit operator SystemTypes(CustomSystemTypes self) => self.systemType;

    public static explicit operator byte(CustomSystemTypes self) => (byte) self.systemType;

    public bool Equals(CustomSystemTypes other) => systemType == other.systemType;

    public override bool Equals(object obj) => obj is CustomSystemTypes other && Equals(other);

    public override int GetHashCode() => (int) systemType;

    public static bool operator ==(CustomSystemTypes left, CustomSystemTypes right) => left.Equals(right);

    public static bool operator !=(CustomSystemTypes left, CustomSystemTypes right) => !left.Equals(right);

    #endregion

    #region Enum members

    // ReSharper disable InconsistentNaming
    public const int MINIMUM = 0x80;
    public const int MAXIMUM = 0x99;

    // Rooms
    [UsedImplicitly]
    public static readonly CustomSystemTypes Research = new(0x80, CustomStringNames.Research);

    [UsedImplicitly]
    public static readonly CustomSystemTypes Observatory = new(0x81, CustomStringNames.Observatory);

    [UsedImplicitly]
    public static readonly CustomSystemTypes UpperCentral = new(0x82, CustomStringNames.UpperCentral);

    [UsedImplicitly]
    public static readonly CustomSystemTypes UpperLobby = new(0x83, CustomStringNames.UpperLobby);

    [UsedImplicitly]
    public static readonly CustomSystemTypes Filtration = new(0x84, CustomStringNames.Filtration);

    [UsedImplicitly]
    public static readonly CustomSystemTypes Ballast = new(0x85, CustomStringNames.Ballast);

    [UsedImplicitly]
    public static readonly CustomSystemTypes LowerCentral = new(0x86, CustomStringNames.LowerCentral);

    [UsedImplicitly]
    public static readonly CustomSystemTypes LowerLobby = new(0x87, CustomStringNames.LowerLobby);

    [UsedImplicitly]
    public static readonly CustomSystemTypes ElevatorHallwayLeft = new(0x88, CustomStringNames.ElevatorsWest);

    [UsedImplicitly]
    public static readonly CustomSystemTypes ElevatorHallwayRight = new(0x89, CustomStringNames.ElevatorsWest);

    [UsedImplicitly]
    public static readonly CustomSystemTypes ElevatorLobbyLeft = new(0x8a, CustomStringNames.ElevatorsEast);

    [UsedImplicitly]
    public static readonly CustomSystemTypes ElevatorLobbyRight = new(0x8b, CustomStringNames.ElevatorsEast);

    [UsedImplicitly]
    public static readonly CustomSystemTypes ElevatorService = new(0x8c, CustomStringNames.ElevatorService);

    // Room-less sabotages
    [UsedImplicitly]
    public static readonly CustomSystemTypes SubmarineFloor = new(0x8d);

    [UsedImplicitly]
    public static readonly CustomSystemTypes SecuritySabotage = new(0x8e);

    [UsedImplicitly]
    public static readonly CustomSystemTypes SpawnIn = new(0x8f);

    [UsedImplicitly]
    public static readonly CustomSystemTypes BoxCat = new(0x90);

    // ReSharper restore InconsistentNaming

    #endregion
}
