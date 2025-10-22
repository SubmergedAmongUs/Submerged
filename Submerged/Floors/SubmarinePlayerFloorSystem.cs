using System.Collections.Generic;
using Hazel;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using AU = Submerged.BaseGame.Interfaces.AU;

namespace Submerged.Floors;

[RegisterInIl2Cpp(typeof(ISystemType))]
public sealed class SubmarinePlayerFloorSystem(nint ptr) : CppObject(ptr), AU.ISystemType
{
    public readonly Dictionary<byte, int> playerFloorSids = new(); // On Upper Deck

    public readonly Dictionary<byte, bool> playerFloorStates = new(); // On Upper Deck

    public SubmarinePlayerFloorSystem() : this(ClassInjector.DerivedConstructorPointer<SubmarinePlayerFloorSystem>())
    {
        ClassInjector.DerivedConstructorBody(this);
        Instance = this;
    }

    public static SubmarinePlayerFloorSystem Instance { get; private set; }

    public bool IsDirty { get; set; }

    public void Deteriorate(float deltaTime)
    {
        if (playerFloorStates.Count == GameData.Instance.AllPlayers.Count) return;

        foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            playerFloorStates[player.PlayerId] = false;
        }
    }

    public void Deserialize(MessageReader reader, bool initialState)
    {
        byte size = reader.ReadByte();

        for (int i = 0; i < size; i++)
        {
            byte id = reader.ReadByte();
            bool state = reader.ReadBoolean();
            playerFloorStates[id] = state;
        }
    }

    public void MarkClean()
    {
        IsDirty = false;
    }

    public void Serialize(MessageWriter writer, bool initialState)
    {
        writer.Write((byte) playerFloorStates.Count);

        foreach (KeyValuePair<byte, bool> pair in playerFloorStates)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value);
        }

        IsDirty = initialState;
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader) { }

    public void ChangePlayerFloorState(byte playerId, bool state)
    {
        IsDirty = true;
        playerFloorStates[playerId] = state;
    }
}
