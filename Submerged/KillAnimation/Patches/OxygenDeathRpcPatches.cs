using HarmonyLib;
using InnerNet;
using Submerged.Enums;

namespace Submerged.KillAnimation.Patches;

public static class OxygenDeathRpcPatches
{
    public static bool MurderPlayerAsOxygenDeath { get; set; }

    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartRpcImmediately))]
    [HarmonyPrefix]
    public static void SendOxygenRpcPatch([HarmonyArgument(1)] ref byte callId)
    {
        if (MurderPlayerAsOxygenDeath && callId == (byte) RpcCalls.MurderPlayer) callId = CustomRpcCalls.OxygenDeath;
        MurderPlayerAsOxygenDeath = false;
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static void HandleOxygenRpcPatch([HarmonyArgument(0)] ref byte callId)
    {
        if (callId == CustomRpcCalls.OxygenDeath) callId = (byte) RpcCalls.MurderPlayer;
    }
}
