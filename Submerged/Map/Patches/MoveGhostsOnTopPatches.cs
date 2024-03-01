using HarmonyLib;
using Submerged.Extensions;
using UnityEngine;

namespace Submerged.Map.Patches;

[HarmonyPatch]
public static class MoveGhostsOnTopPatches
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.LateUpdate))]
    [HarmonyPostfix]
    public static void ForceGhostZPosPatch(PlayerPhysics __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;
        if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data is not { IsDead: false }) return;

        PlayerControl otherPlayer = __instance.myPlayer;
        GameData.PlayerInfo otherPlayerData = otherPlayer.Data;

        if (otherPlayerData is not { IsDead: true }) return;

        Transform transform = __instance.transform;
        Vector3 pos = transform.position;
        pos.z = -5f;
        transform.position = pos;
    }
}
