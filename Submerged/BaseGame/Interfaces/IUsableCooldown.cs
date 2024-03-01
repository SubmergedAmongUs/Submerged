namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    public interface IUsableCoolDown : IUsable
    {
        float CoolDown
        {
            get;
            set;
        }

        float MaxCoolDown
        {
            get;
        }

        bool IsCoolingDown();
    }
}
