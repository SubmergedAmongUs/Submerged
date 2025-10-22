using System;
using System.Collections.Generic;
using Submerged.Localization.Strings;

namespace Submerged.Enums;

public readonly struct CustomStringNames
{
    #region Struct implementation

    public static List<CustomStringNames> All { get; } = [];
    private static readonly Dictionary<StringNames, CustomStringNames> _mapping = new();

    public readonly StringNames stringName;
    public readonly Func<string> getter;

    private CustomStringNames(int stringName, Func<string> getter)
    {
        this.stringName = (StringNames) stringName;
        this.getter = getter;

        All.Add(this);
        _mapping.Add(this.stringName, this);
    }

    private CustomStringNames(int stringName, string constantValue) : this(stringName, () => constantValue) { }

    public static bool TryGetFromStringName(StringNames stringName, out CustomStringNames result) => _mapping.TryGetValue(stringName, out result);

    public static implicit operator StringNames(CustomStringNames customStringName) => customStringName.stringName;

    #endregion

    #region Enum members

    // ReSharper disable InconsistentNaming

    public const int MINIMUM = 555_0_000;
    public const int MAXIMUM = 555_9_999;

    // General
    public static readonly CustomStringNames Submerged = new(555_0_000, "Submerged");
    public static readonly CustomStringNames SubmergedTooltip = new(555_0_001, () => General.SubmergedTooltip);

    // Rooms
    public static readonly CustomStringNames Research = new(555_1_000, () => Locations.Research);
    public static readonly CustomStringNames Observatory = new(555_1_001, () => Locations.Observatory);
    public static readonly CustomStringNames UpperCentral = new(555_1_002, () => Locations.Central_Upper);
    public static readonly CustomStringNames UpperLobby = new(555_1_003, () => Locations.Lobby_Upper);
    public static readonly CustomStringNames Filtration = new(555_1_004, () => Locations.Filtration);
    public static readonly CustomStringNames Ballast = new(555_1_005, () => Locations.Ballast);
    public static readonly CustomStringNames LowerCentral = new(555_1_006, () => Locations.Central_Lower);
    public static readonly CustomStringNames LowerLobby = new(555_1_007, () => Locations.Lobby_Lower);
    public static readonly CustomStringNames Elevator = new(555_1_008, () => Locations.Elevator);
    public static readonly CustomStringNames ElevatorService = new(555_1_009, () => Locations.Elevator_Service);

    // Tasks
    public static readonly CustomStringNames PlugLeaks = new(555_2_000, () => Tasks.PlugLeaks);
    public static readonly CustomStringNames SpotWhaleShark = new(555_2_001, () => Tasks.SpotWhaleShark);
    public static readonly CustomStringNames MicrowaveLunch = new(555_2_002, () => Tasks.MicrowaveLunch);
    public static readonly CustomStringNames ReshelveBooks = new(555_2_003, () => Tasks.ReshelveBooks);
    public static readonly CustomStringNames RecordNavBeaconData = new(555_2_004, () => Tasks.RecordNavBeaconData);
    public static readonly CustomStringNames MopPuddles = new(555_2_005, () => Tasks.MopPuddles);
    public static readonly CustomStringNames OxygenateSeaPlants = new(555_2_006, () => Tasks.OxygenateSeaPlants);
    public static readonly CustomStringNames ClearUrchins = new(555_2_007, () => Tasks.ClearUrchins);
    public static readonly CustomStringNames ShootDepthCharges = new(555_2_008, () => Tasks.ShootDepthCharges);
    public static readonly CustomStringNames DiagnoseElevators = new(555_2_009, () => Tasks.DiagnoseElevators);
    public static readonly CustomStringNames PurchaseBreakfast = new(555_2_010, () => Tasks.PurchaseBreakfast);
    public static readonly CustomStringNames ReconnectPiping = new(555_2_011, () => Tasks.ReconnectPiping);
    public static readonly CustomStringNames CleanGlass = new(555_2_012, () => Tasks.CleanGlass);
    public static readonly CustomStringNames IdentifySpecimen = new(555_2_013, () => Tasks.IdentifySpecimen);
    public static readonly CustomStringNames StartSubmersible = new(555_2_014, () => Tasks.StartSubmersible);
    public static readonly CustomStringNames TrackMantaRay = new(555_2_015, () => Tasks.TrackMantaRay);
    public static readonly CustomStringNames DispenseWater = new(555_2_016, () => Tasks.DispenseWater);
    public static readonly CustomStringNames SteadyHeartbeat = new(555_2_017, () => Tasks.SteadyHeartbeat);
    public static readonly CustomStringNames FeedPetFish = new(555_2_018, () => Tasks.FeedPetFish);
    public static readonly CustomStringNames SortScubaGear = new(555_2_019, () => Tasks.SortScubaGear);
    public static readonly CustomStringNames CycleReactor = new(555_2_020, () => Tasks.CycleReactor);
    public static readonly CustomStringNames ResetBreakers = new(555_2_021, () => Tasks.ResetBreakers);
    public static readonly CustomStringNames LocateVolcanicActivity = new(555_2_022, () => Tasks.LocateVolcanicActivity);
    public static readonly CustomStringNames RetrieveOxygenMask = new(555_2_023, () => Tasks.RetrieveOxygenMask);
    public static readonly CustomStringNames StabilizeWaterLevels = new(555_2_024, () => Tasks.StabilizeWaterLevels);

    // ReSharper restore InconsistentNaming

    #endregion
}
