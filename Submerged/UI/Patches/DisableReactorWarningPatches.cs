using System.Linq;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class DisableReactorWarningPatches
{
    private static readonly SCG.HashSet<string> _allowedPlugins =
    [
        "com.sinai.unityexplorer",
        "gg.reactor.api",
        "Submerged"
    ];

    [HarmonyPatch(typeof(DisconnectPopup), nameof(DisconnectPopup.ShowCustom))]
    [HarmonyPrefix]
    public static bool PreventReactorPopupPatch([HarmonyArgument(0)] string message)
    {
        // ReSharper disable once ArrangeMethodOrOperatorBody
        return !message.Contains("modded handshake") || HasOtherPlugins();
    }

    private static bool HasOtherPlugins() => !IL2CPPChainloader.Instance.Plugins.Keys.All(_allowedPlugins.Contains);
}
