using System.Collections.Generic;
using System.Linq;
using Hazel;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using Submerged.Extensions;
using Submerged.SpawnIn.Enums;
using UnityEngine;
using AU = Submerged.BaseGame.Interfaces.AU;

namespace Submerged.SpawnIn;

[RegisterInIl2Cpp(typeof(ISystemType))]
public sealed class SubmarineSpawnInSystem(nint ptr) : CppObject(ptr), AU.ISystemType
{
    public SpawnInState currentState = SpawnInState.Loading;
    public HashSet<byte> players = [];
    public float timer = 10;

    public SubmarineSpawnInSystem() : this(ClassInjector.DerivedConstructorPointer<SubmarineSpawnInSystem>())
    {
        ClassInjector.DerivedConstructorBody(this);
        Instance = this;
    }

    public static SubmarineSpawnInSystem Instance { get; private set; }

    public bool IsDirty { get; set; }

    public void Deteriorate(float deltaTime)
    {
        if (currentState == SpawnInState.Spawning)
        {
            timer = Mathf.Max(0, timer - deltaTime);
        }

        foreach (NetworkedPlayerInfo instanceAllPlayer in GameData.Instance.AllPlayers.GetFastEnumerator())
        {
            if (instanceAllPlayer.IsDead || instanceAllPlayer.Disconnected || instanceAllPlayer.Object.isDummy) continue;
            if (players.Contains(instanceAllPlayer.PlayerId)) continue;

            return;
        }

        currentState++;
        players.Clear();
        timer = 10;
        IsDirty = true;
    }

    public void Deserialize(MessageReader reader, bool initialState)
    {
        currentState = (SpawnInState) reader.ReadByte();
        players = reader.ReadBytesAndSize().ToHashSet();
        timer = reader.ReadSingle();
    }

    public void MarkClean()
    {
        IsDirty = false;
    }

    public void Serialize(MessageWriter writer, bool initialState)
    {
        writer.Write((byte) currentState);
        writer.WriteBytesAndSize(players.ToArray());
        writer.Write(timer);
        IsDirty = initialState || currentState != SpawnInState.Done;
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
        players.Add(player.PlayerId);
        IsDirty = true;
    }

    public int GetReadyPlayerAmount() => players.Count(p => GameData.Instance.GetPlayerById(p) is { IsDead: false, Disconnected: false, Object.isDummy: false });

    public int GetTotalPlayerAmount()
    {
        int count = 0;

        foreach (NetworkedPlayerInfo instanceAllPlayer in GameData.Instance.AllPlayers)
        {
            if (!instanceAllPlayer || !instanceAllPlayer.Object ||
                instanceAllPlayer.IsDead || instanceAllPlayer.Disconnected || instanceAllPlayer.Object.isDummy) continue;
            count++;
        }

        return count;
    }
}
