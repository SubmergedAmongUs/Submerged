using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using Submerged.Enums;
using UnityEngine;

namespace Submerged.KillAnimation.Patches;

[HarmonyPatch]
public static class OxygenDeathAnimationPatches
{
    private static OverlayKillAnimation _oxygenDeath;

    private static OverlayKillAnimation OxygenDeath
    {
        get
        {
            // DO NOT TOUCH THIS GETTER
            // LITERALLY DO NOT TOUCH IT
            //
            // The objects used in this method are in some kind of ethereal state.
            // After very careful manipulation and a lot of time and patience,
            // I have managed to come up with a very meticulous recipe for modifying
            // the death animation. If you change this... you will pay with your blood!
            //
            // - Alex

            if (_oxygenDeath) return _oxygenDeath;

            Transform parent = new GameObject("Submerged OxygenDeathParent").DontUnload().DontDestroy().transform;
            parent.gameObject.SetActive(false);
            _oxygenDeath = UnityObject.Instantiate(HudManager.Instance.KillOverlay.KillAnims[0], parent);

            _oxygenDeath.killerParts.gameObject.SetActive(false);
            _oxygenDeath.killerParts = null;
            _oxygenDeath.transform.Find("killstabknife").gameObject.SetActive(false);
            _oxygenDeath.transform.Find("killstabknifehand").gameObject.SetActive(false);

            _oxygenDeath.victimParts.transform.localPosition = new Vector3(-1.5f, 0, 0);
            _oxygenDeath.KillType = CustomKillAnimTypes.Oxygen;

            _oxygenDeath.gameObject.AddComponent<CustomKillAnimationPlayer>();

            return _oxygenDeath;
        }
    }

    public static bool IsOxygenDeath { get; set; }

    [HarmonyPatch(typeof(KillOverlay), nameof(KillOverlay.ShowKillAnimation), typeof(GameData.PlayerInfo), typeof(GameData.PlayerInfo))]
    [HarmonyPrefix]
    public static bool ShowOxygenKillAnimationPatch(KillOverlay __instance, [HarmonyArgument(0)] GameData.PlayerInfo killer, [HarmonyArgument(1)] GameData.PlayerInfo victim)
    {
        if (killer.PlayerId != victim.PlayerId || !IsOxygenDeath || AprilFoolsMode.ShouldHorseAround() || AprilFoolsMode.ShouldLongAround()) return true;

        __instance.ShowKillAnimation(OxygenDeath, killer, victim);

        return false;
    }

    [HarmonyPatch(typeof(OverlayKillAnimation), nameof(OverlayKillAnimation.WaitForFinish))]
    [HarmonyPrefix]
    public static bool WaitForCustomAnimationFinishPatch(OverlayKillAnimation __instance, ref CppIEnumerator __result)
    {
        CustomKillAnimationPlayer customKillAnim = __instance.GetComponent<CustomKillAnimationPlayer>();

        if (!customKillAnim) return true;
        __result = customKillAnim.WaitForFinish().WrapToIl2Cpp();

        return false;
    }
}
