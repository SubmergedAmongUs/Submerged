namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v2025_5_20)]
    public interface IDoorMinigame
    {
        [UsedImplicitly]
        void SetDoor(OpenableDoor door);
    }
}
