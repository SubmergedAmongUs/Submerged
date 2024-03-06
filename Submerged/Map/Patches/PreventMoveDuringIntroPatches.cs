using HarmonyLib;
using Submerged.Extensions;
using IntroCutscene_CoBegin = IntroCutscene._CoBegin_d__35;

namespace Submerged.Map.Patches;

[HarmonyPatch]
public static class PreventMoveDuringIntroPatches
{
    [HarmonyPatch(typeof(IntroCutscene_CoBegin), nameof(IntroCutscene_CoBegin.MoveNext))]
    [HarmonyPrefix]
    public static void AssignIntroCutsceneInstancePatch(IntroCutscene_CoBegin __instance, ref bool __result)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        if (__instance.__1__state == 0)
        {
            // Bug in Among Us, this isn't assigned
            IntroCutscene.Instance = __instance.__4__this;
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CanMove), MethodType.Getter)]
    [HarmonyPostfix]
    public static void PreventPlayerMovingDuringShhhPatch(ref bool __result)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        __result = __result && (!HudManager.InstanceExists || !HudManager.Instance.shhhEmblem.isActiveAndEnabled);
    }
}
