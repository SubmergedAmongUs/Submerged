using System;

namespace Submerged.BaseGame;

public enum LastChecked
{
    // ReSharper disable InconsistentNaming
    v2025_3_31,
    // ReSharper restore InconsistentNaming
}

[AttributeUsage(AttributeTargets.All)]
public class BaseGameCodeAttribute : Attribute
{
    [UsedImplicitly]
    public BaseGameCodeAttribute(LastChecked version, string notes = "") { }
}
