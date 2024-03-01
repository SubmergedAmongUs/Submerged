using HarmonyLib;
using Submerged.Extensions;

namespace Submerged.Map.Patches;

[HarmonyPatch]
public static class FixLayeringIssuesPatches
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void UpdateCosmeticsZPatch(PlayerControl __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        __instance.cosmetics.visor.transform.SetZLocalPos(-0.0003f);
        __instance.cosmetics.hat.FrontLayer.transform.SetZLocalPos(-0.0002f);
        __instance.cosmetics.skin.transform.SetZLocalPos(-0.0001f);

        __instance.cosmetics.hat.BackLayer.transform.SetZLocalPos(0.0001f);
    }
}
