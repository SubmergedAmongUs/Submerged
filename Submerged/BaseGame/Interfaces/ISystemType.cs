using Hazel;

namespace Submerged.BaseGame.Interfaces;

// ReSharper disable once InconsistentNaming
public sealed partial class AU
{
    [BaseGameCode(LastChecked.v2024_3_5)]
    public interface ISystemType
    {
        [UsedImplicitly]
        bool IsDirty { get; }

        [UsedImplicitly]
        void Deteriorate(float deltaTime);

        [UsedImplicitly]
        void UpdateSystem(PlayerControl player, MessageReader msgReader);

        [UsedImplicitly]
        void Serialize(MessageWriter writer, bool initialState);

        [UsedImplicitly]
        void Deserialize(MessageReader reader, bool initialState);
    }
}
