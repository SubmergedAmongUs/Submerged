using Hazel;

namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v2023_11_28)]
    public interface IDoorMinigame
    {
        [UsedImplicitly]
        void SetDoor(OpenableDoor door);
    }
}
