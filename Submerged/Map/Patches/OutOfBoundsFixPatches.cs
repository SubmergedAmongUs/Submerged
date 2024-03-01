using HarmonyLib;

namespace Submerged.Map.Patches;

[HarmonyPatch]
public static class OutOfBoundsFixPatches
{
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.ResetMoveState))]
    [HarmonyPostfix]
    public static void EnableGhostColliderPatch(PlayerPhysics __instance)
    {
        if (__instance.myPlayer && __instance.myPlayer.Data is { IsDead: true } && __instance.myPlayer.Collider)
        {
            // Bug in Among Us
            __instance.myPlayer.Collider.enabled = true;
        }
    }
}
