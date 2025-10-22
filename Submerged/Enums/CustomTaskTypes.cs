using System;
using System.Collections.Generic;
using static NormalPlayerTask.TaskLength;
using static Submerged.Enums.CustomTaskTypes.Floor;

namespace Submerged.Enums;

public readonly struct CustomTaskTypes : IEquatable<CustomTaskTypes>
{
    #region Struct implementation

    public static List<CustomTaskTypes> All { get; } = [];
    private static readonly Dictionary<TaskTypes, CustomTaskTypes> _mapping = new();

    public readonly TaskTypes taskType;
    public readonly StringNames stringName;
    public readonly Floor floor;
    public readonly NormalPlayerTask.TaskLength length;

    private CustomTaskTypes(int taskType, StringNames stringName, Floor floor, NormalPlayerTask.TaskLength length)
    {
        this.taskType = (TaskTypes) taskType;
        this.stringName = stringName;
        this.floor = floor;
        this.length = length;

        All.Add(this);
        _mapping.Add(this.taskType, this);
    }

    private CustomTaskTypes(TaskTypes taskType, Floor floor, NormalPlayerTask.TaskLength length) : this((int) taskType, 0, floor, length) { }

    public static bool TryGetFromTaskType(TaskTypes taskTypes, out CustomTaskTypes result) => _mapping.TryGetValue(taskTypes, out result);

    public static implicit operator TaskTypes(CustomTaskTypes self) => self.taskType;

    public enum Floor
    {
        LowerDeck = 0b01,
        UpperDeck = 0b10,
        Both = LowerDeck | UpperDeck
    }

    public bool Equals(CustomTaskTypes other) => taskType == other.taskType;

    public override bool Equals(object obj) => obj is CustomTaskTypes other && Equals(other);

    public override int GetHashCode() => (int) taskType;

    public static bool operator ==(CustomTaskTypes left, CustomTaskTypes right) => left.Equals(right);

    public static bool operator !=(CustomTaskTypes left, CustomTaskTypes right) => !left.Equals(right);

    #endregion

    #region Enum members

    // ReSharper disable InconsistentNaming
    public const int MINIMUM = 0x80;
    public const int MAXIMUM = 0x99;

    // Base-game tasks
    [UsedImplicitly]
    public static readonly CustomTaskTypes UploadData = new(TaskTypes.UploadData, UpperDeck, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes FixWiring = new(TaskTypes.FixWiring, LowerDeck, Common);

    [UsedImplicitly]
    public static readonly CustomTaskTypes AlignTelescope = new(TaskTypes.AlignTelescope, UpperDeck, Short);

    // Custom tasks
    [UsedImplicitly]
    public static readonly CustomTaskTypes PlugLeaks = new(0x80, CustomStringNames.PlugLeaks, Both, Common);

    [UsedImplicitly]
    public static readonly CustomTaskTypes SpotWhaleShark = new(0x81, CustomStringNames.SpotWhaleShark, UpperDeck, Common);

    [UsedImplicitly]
    public static readonly CustomTaskTypes MicrowaveLunch = new(0x82, CustomStringNames.MicrowaveLunch, UpperDeck, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes ReshelveBooks = new(0x83, CustomStringNames.ReshelveBooks, UpperDeck, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes RecordNavBeaconData = new(0x84, CustomStringNames.RecordNavBeaconData, UpperDeck, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes MopPuddles = new(0x85, CustomStringNames.MopPuddles, LowerDeck, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes OxygenateSeaPlants = new(0x86, CustomStringNames.OxygenateSeaPlants, LowerDeck, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes ClearUrchins = new(0x87, CustomStringNames.ClearUrchins, LowerDeck, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes ShootDepthCharges = new(0x88, CustomStringNames.ShootDepthCharges, LowerDeck, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes DiagnoseElevators = new(0x89, CustomStringNames.DiagnoseElevators, Both, Long);

    [UsedImplicitly]
    public static readonly CustomTaskTypes PurchaseBreakfast = new(0x8B, CustomStringNames.PurchaseBreakfast, UpperDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes ReconnectPiping = new(0x8C, CustomStringNames.ReconnectPiping, LowerDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes CleanGlass = new(0x8D, CustomStringNames.CleanGlass, UpperDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes IdentifySpecimen = new(0x8E, CustomStringNames.IdentifySpecimen, UpperDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes StartSubmersible = new(0x8F, CustomStringNames.StartSubmersible, LowerDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes TrackMantaRay = new(0x90, CustomStringNames.TrackMantaRay, UpperDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes DispenseWater = new(0x91, CustomStringNames.DispenseWater, LowerDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes SteadyHeartbeat = new(0x92, CustomStringNames.SteadyHeartbeat, UpperDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes FeedPetFish = new(0x93, CustomStringNames.FeedPetFish, UpperDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes SortScubaGear = new(0x94, CustomStringNames.SortScubaGear, LowerDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes CycleReactor = new(0x95, CustomStringNames.CycleReactor, LowerDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes ResetBreakers = new(0x96, CustomStringNames.ResetBreakers, LowerDeck, Short);

    [UsedImplicitly]
    public static readonly CustomTaskTypes LocateVolcanicActivity = new(0x97, CustomStringNames.LocateVolcanicActivity, UpperDeck, Short);

    // Custom sabotages
    [UsedImplicitly]
    public static readonly CustomTaskTypes RetrieveOxygenMask = new(0x98, CustomStringNames.RetrieveOxygenMask, Both, None);

    // ReSharper restore InconsistentNaming

    #endregion
}
