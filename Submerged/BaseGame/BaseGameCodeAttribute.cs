using System;

namespace Submerged.BaseGame;

public enum LastChecked
{
    // ReSharper disable InconsistentNaming
    v17_0_0,
    v17_0_1
    // ReSharper restore InconsistentNaming
}

[AttributeUsage(AttributeTargets.All)]
public class BaseGameCodeAttribute : Attribute
{
    [UsedImplicitly]
    public BaseGameCodeAttribute(LastChecked version, string notes = "") { }
}
