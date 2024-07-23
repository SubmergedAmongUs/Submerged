namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v2024_6_18)]
    public interface IDoorMinigame
    {
        [UsedImplicitly]
        void SetDoor(OpenableDoor door);
    }
}
