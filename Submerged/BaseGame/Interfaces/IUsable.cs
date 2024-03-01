namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v2023_10_24)]
    public interface IUsable
    {
        [UsedImplicitly]
        float UsableDistance { get; }

        [UsedImplicitly]
        float PercentCool { get; }

        [UsedImplicitly]
        ImageNames UseIcon { get; }

        [UsedImplicitly]
        void SetOutline(bool on, bool mainTarget);

        [UsedImplicitly]
        float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse);

        [UsedImplicitly]
        void Use();
    }
}
