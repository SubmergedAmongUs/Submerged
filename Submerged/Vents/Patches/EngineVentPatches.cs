using HarmonyLib;
using Submerged.Extensions;
using Submerged.Floors;
using UnityEngine;
using static Submerged.Vents.VentPatchData;

namespace Submerged.Vents.Patches;

[HarmonyPatch]
public static class EngineVentPatches
{
    private static VentilationSystem VentSystem => ShipStatus.Instance ? ShipStatus.Instance.Systems[SystemTypes.Ventilation].Cast<VentilationSystem>() : null;

    [HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.IsVentCurrentlyBeingCleaned))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void PreventEnteringPatch([HarmonyArgument(0)] int id, ref bool __result)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;
        if (id != ENGINE_ROOM_VENT_ID) return;

        if ((!PlayerControl.LocalPlayer.inVent && !FloorHandler.LocalPlayer.onUpper) || // Prevent entering directly
            (Vent.currentVent && Vent.currentVent.Id != id && IsAlivePersonInsideVent(id))) // Prevent moving if another player is inside
        {
            __result = true;
        }
    }

    private static bool IsAlivePersonInsideVent(int targetVent)
    {
        foreach (ICG.KeyValuePair<byte, byte> kvp in VentSystem.PlayersInsideVents) // can't deconstruct ICG.KVP<byte, byte> 😭
        {
            if (kvp.Value == targetVent)
            {
                NetworkedPlayerInfo player = GameData.Instance.GetPlayerById(kvp.Key);
                if (!player.IsDead && !player.Disconnected) return true;
            }
        }
        return false;
    }

    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.RpcExitVent))]
    [HarmonyPrefix]
    public static void MovePlayerUpWhenExitingPatch(PlayerPhysics __instance, [HarmonyArgument(0)] int id)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;
        if (id != ENGINE_ROOM_VENT_ID) return;

        __instance.ResetMoveState();
        __instance.ResetAnimState();

        PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(PlayerControl.LocalPlayer.transform.position + new Vector3(0, 0.4f));

        SoundManager.Instance.PlaySound(ShipStatus.Instance.VentMoveSounds.Random(), false).pitch = FloatRange.Next(0.8f, 1.2f);
    }
}
