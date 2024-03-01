using HarmonyLib;
using Submerged.Extensions;

namespace Submerged.Systems.SecuritySabotage.Patches;

[HarmonyPatch]
public static class AnimationPatches
{
    [HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.UpdateCameras))]
    [HarmonyPostfix]
    public static void DontShowSabotagedCamerasAsOnPatch(SecurityCameraSystemType __instance)
    {
        if (!ShipStatus.Instance.IsSubmerged()) return;

        for (int i = 0; i < ShipStatus.Instance.AllCameras.Length; i++)
        {
            SurvCamera camera = ShipStatus.Instance.AllCameras[i];
            bool sabotaged = SubmarineSecuritySabotageSystem.Instance.IsSabotaged((byte) i);
            camera.SetAnimation(__instance.InUse && !sabotaged);
        }
    }
}
