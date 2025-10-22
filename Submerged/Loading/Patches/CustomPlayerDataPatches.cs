using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Reactor.Networking.Attributes;
using Submerged.Enums;
using Submerged.Extensions;

namespace Submerged.Loading.Patches;

[HarmonyPatch]
public class CustomPlayerDataPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    public static void SendCustomDataPatch(PlayerControl __instance, ref CppIEnumerator __result)
    {
        __result = CoStartAndSendCustomData(__instance, __result).WrapToIl2Cpp();
    }

    private static IEnumerator CoStartAndSendCustomData(PlayerControl playerControl, CppIEnumerator original)
    {
        yield return original;

        if (!playerControl.AmOwner) yield break;
        bool mapLoaded = !AssetLoader.Errored;

        if (AmongUsClient.Instance.AmClient)
        {
            CustomPlayerData data = playerControl.gameObject.EnsureComponent<CustomPlayerData>();
            data.HasMap = mapLoaded;
        }

        RpcSetCustomData(playerControl, mapLoaded);
    }

    [MethodRpc(CustomRpcCalls.SetCustomData)]
    public static void RpcSetCustomData(PlayerControl player, bool mapLoaded)
    {
        CustomPlayerData data = player.gameObject.EnsureComponent<CustomPlayerData>();
        data.HasMap = mapLoaded;
    }
}
