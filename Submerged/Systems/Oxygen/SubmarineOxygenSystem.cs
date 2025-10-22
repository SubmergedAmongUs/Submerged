using System.Collections.Generic;
using System.Linq;
using Hazel;
using Il2CppInterop.Runtime.Injection;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Attributes;
using Submerged.Enums;
using Submerged.KillAnimation.Patches;
using UnityEngine;
using AU = Submerged.BaseGame.Interfaces.AU;

namespace Submerged.Systems.Oxygen;

[RegisterInIl2Cpp(typeof(ISystemType), typeof(IActivatable))]
public sealed class SubmarineOxygenSystem(nint ptr) : CppObject(ptr), AU.ISystemType
{
    public readonly float duration;

    public float countdown = 10000f;

    public float dirtyTimer;
    public bool doKillCheck;
    public bool doKillCheckSerialized;
    public HashSet<byte> playersWithMask = new();

    public float recentlyActive;

    public SubmarineOxygenSystem(float duration) : this(ClassInjector.DerivedConstructorPointer<SubmarineOxygenSystem>())
    {
        ClassInjector.DerivedConstructorBody(this);

        Instance = this;
        this.duration = duration;
    }

    public static SubmarineOxygenSystem Instance { get; private set; }

    public bool IsActive => countdown < 10000f;
    public int RemainingMasks => GameData.Instance.AllPlayers.Count - playersWithMask.Count;

    public bool IsDirty { get; set; }

    public bool LocalPlayerNeedsMask => !playersWithMask.Contains(PlayerControl.LocalPlayer.PlayerId) && !PlayerControl.LocalPlayer.Data.IsDead;

    public void Deteriorate(float deltaTime)
    {
        HudManager hudManager = HudManager.Instance;
        PlayerControl localPlayer = PlayerControl.LocalPlayer;

        if (doKillCheck)
        {
            doKillCheck = false;

            if (localPlayer && !localPlayer.Data.Disconnected && !localPlayer.Data.IsDead && !playersWithMask.Contains(localPlayer.PlayerId))
            {
                try
                {
                    OxygenDeathAnimationPatches.IsOxygenDeath = true;

                    if (localPlayer.inVent)
                    {
                        Vent.currentVent.SetButtons(false);
                        localPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
                    }
                    RpcOxygenDeath(localPlayer);
                }
                finally
                {
                    OxygenDeathAnimationPatches.IsOxygenDeath = false;
                }
            }
        }

        if (IsActive)
        {
            recentlyActive = 0.5f;

            if (hudManager.OxyFlash == null) localPlayer.AddSystemTask(SystemTypes.LifeSupp);

            if (playersWithMask.Contains(localPlayer.PlayerId) && hudManager.FullScreen.color.a != 0.1f)
            {
                Color color = new Color32(252, 173, 3, 25);
                color.a = 0.1f;
                hudManager.FullScreen.color = color;
            }

            countdown -= deltaTime;
            dirtyTimer += deltaTime;

            if (RemainingMasks == 0)
            {
                countdown = 10000f;
                IsDirty = true;
            }
            else if (countdown < 0)
            {
                countdown = 10000f;
                doKillCheckSerialized = doKillCheck = true;
                IsDirty = true;
            }
            else if (dirtyTimer > 2)
            {
                dirtyTimer = 0f;
                IsDirty = true;
            }
        }
        else
        {
            recentlyActive -= deltaTime;

            if (HudManager.Instance.OxyFlash != null)
            {
                HudManager.Instance.StopOxyFlash();
            }
        }
    }

    public void Deserialize(MessageReader reader, bool initialState)
    {
        countdown = reader.ReadSingle();
        playersWithMask = reader.ReadBytesAndSize().ToHashSet();
        doKillCheck = reader.ReadBoolean();
    }

    public void MarkClean()
    {
        IsDirty = false;
    }

    public void Serialize(MessageWriter writer, bool initialState)
    {
        writer.Write(countdown);
        writer.WriteBytesAndSize(playersWithMask.ToArray());
        writer.Write(doKillCheckSerialized);
        doKillCheckSerialized = false;

        IsDirty = initialState;
    }

    public void RepairDamage(PlayerControl player, byte amount)
    {
        switch (amount)
        {
            case 16: // Stop sabotage
                countdown = 10000f;
                playersWithMask.Clear();
                doKillCheck = doKillCheckSerialized = false;

                break;

            case 64: // Give player mask
                playersWithMask.Add(player.PlayerId);

                break;

            case 128 when !IsActive: // Start sabotage
                countdown = duration;
                playersWithMask.Clear();

                break;
        }

        IsDirty = true;
    }

    public void UpdateSystem(PlayerControl player, MessageReader msgReader)
    {
        byte amount = msgReader.ReadByte();
        RepairDamage(player, amount);
    }

    [MethodRpc(CustomRpcCalls.OxygenDeath)]
    public static void RpcOxygenDeath(PlayerControl player)
    {
        try
        {
            if (player.AmOwner) KillCooldownPatches.PreventReset = true;
            player.MurderPlayer(player, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
        }
        finally
        {
            KillCooldownPatches.PreventReset = false;
        }
    }
}
