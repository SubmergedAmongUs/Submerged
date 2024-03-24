using System;

namespace Submerged.BaseGame;

public enum LastChecked
{
    // ReSharper disable InconsistentNaming
    v2024_3_5
    // ReSharper restore InconsistentNaming
}

[AttributeUsage(AttributeTargets.All)]
public class BaseGameCodeAttribute : Attribute
{
    [UsedImplicitly]
    public BaseGameCodeAttribute(LastChecked version, string notes = "") { }
}
