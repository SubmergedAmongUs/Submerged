using HarmonyLib;
using Submerged.Extensions;

namespace Submerged.Minigames.Patches;

[HarmonyPatch(typeof(HudOverrideTask), nameof(HudOverrideTask.FixedUpdate))]
public static class HudOverrideTaskFixedUpdatePatch
{
    [HarmonyPostfix]
    public static void Postfix(HudOverrideTask __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        if (!__instance.isComplete)
        {
            __instance.HasLocation = true;
        }
    }
}
