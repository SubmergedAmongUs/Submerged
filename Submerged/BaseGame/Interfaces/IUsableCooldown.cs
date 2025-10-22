namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v17_0_0)]
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
