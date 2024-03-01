using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using InnerNet;
using Submerged.Enums;

namespace Submerged.Floors.Patches;

[HarmonyPatch]
public static class RpcPatches
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool HandleRequestChangeFloorPatch(PlayerPhysics __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch (callId)
        {
            case CustomRpcCalls.RequestChangeFloor: // RpcCalls RequestChangeFloor // Bool - Upper Deck // PackedInt32 - Nonce
                if (!AmongUsClient.Instance.AmHost) return false;

                SubmarinePlayerFloorSystem floorSystem = SubmarinePlayerFloorSystem.Instance;
                bool state = reader.ReadBoolean();
                int sid = reader.ReadInt32();

                if (!floorSystem.playerFloorSids.ContainsKey(__instance.myPlayer.PlayerId) || floorSystem.playerFloorSids[__instance.myPlayer.PlayerId] <= sid)
                {
                    floorSystem.playerFloorSids[__instance.myPlayer.PlayerId] = sid;
                    floorSystem!.ChangePlayerFloorState(__instance.myPlayer.PlayerId, state);
                }

                SubmarinePlayerFloorSystem.RespondToFloorChange(__instance, sid);

                return false;

            default:
                return true;
        }
    }

    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool HandleAcknowledgeChangeFloorPatch(ShipStatus __instance, [HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        switch (callId)
        {
            case CustomRpcCalls.AcknowledgeChangeFloor:
                PlayerPhysics physics = reader.ReadNetObject<PlayerPhysics>();

                if (!physics.AmOwner) return false;

                FloorHandler floorManager = FloorHandler.GetFloorHandler(physics);

                // TODO: Wth is this code doing
                List<int> newList = floorManager.ints.ToList();
                if (newList.Any()) newList.RemoveAt(0);
                floorManager.ints = newList;

                return false;

            default:
                return true;
        }
    }
}
