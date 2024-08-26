namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v2024_8_13)]
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
