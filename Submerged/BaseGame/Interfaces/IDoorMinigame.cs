namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v2024_3_5)]
    public interface IDoorMinigame
    {
        [UsedImplicitly]
        void SetDoor(OpenableDoor door);
    }
}
