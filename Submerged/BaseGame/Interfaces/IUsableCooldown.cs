namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v2025_5_20)]
    public interface IUsableCoolDown : IUsable
    {
        [UsedImplicitly]
        float CoolDown
        {
            get;
            set;
        }

        [UsedImplicitly]
        float MaxCoolDown
        {
            get;
        }

        [UsedImplicitly]
        bool IsCoolingDown();
    }
}
