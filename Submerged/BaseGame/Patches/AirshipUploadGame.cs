using HarmonyLib;
using UnityEngine;

namespace Submerged.BaseGame.Patches;

[HarmonyPatch]
public static class AirshipUploadGameUpdatePatches
{
    // TODO: This patch may not be needed, and if it is it should be documented why!
    [HarmonyPatch(typeof(AirshipUploadGame), nameof(AirshipUploadGame.Update))]
    [HarmonyPrefix]
    [BaseGameCode(LastChecked.v2025_5_20, "Patching the method with it's own code because C# > C++")]
    public static void Prefix(AirshipUploadGame __instance, out bool __runOriginal)
    {
        __runOriginal = false;

        if (__instance.amClosing != Minigame.CloseState.None)
        {
            return;
        }

        float num = 0f;

        if (__instance.Hotspot.IsTouching(__instance.Perfect))
        {
            if (!__instance.PerfectIcon.activeSelf)
            {
                __instance.DeactivateIcons();
                __instance.PerfectIcon.SetActive(true);
            }

            num = Time.deltaTime * 4f;
        }
        else if (__instance.Hotspot.IsTouching(__instance.Good))
        {
            if (!__instance.GoodIcon.activeSelf)
            {
                __instance.DeactivateIcons();
                __instance.GoodIcon.SetActive(true);
            }

            num = Time.deltaTime * 2f;
        }
        else if (__instance.Hotspot.IsTouching(__instance.Poor))
        {
            if (!__instance.PoorIcon.activeSelf)
            {
                __instance.DeactivateIcons();
                __instance.PoorIcon.SetActive(true);
            }

            num = Time.deltaTime;
        }
        else if (!__instance.NoneIcon.activeSelf)
        {
            __instance.DeactivateIcons();
            __instance.NoneIcon.SetActive(true);
        }

        __instance.cont.Update();

        if (__instance.glyphColor.a > 0f)
        {
            if (__instance.glyphDisappearDelay > 0f)
            {
                __instance.glyphDisappearDelay -= Time.deltaTime;
            }
            else
            {
                __instance.promptGlyph.color = __instance.glyphColor;
                Color x = __instance.glyphColor;
                x.a = __instance.glyphColor.a - Time.deltaTime;
                __instance.glyphColor = x;
            }
        }
        else if (__instance.promptGlyph.gameObject.activeSelf)
        {
            __instance.promptGlyph.gameObject.SetActive(false);
        }

        if (Controller.currentTouchType == Controller.TouchType.Joystick)
        {
            __instance.Phone.transform.position = VirtualCursor.currentPosition;
        }

        __instance.timer += num;

        if (Constants.ShouldPlaySfx())
        {
            __instance.beepTimer += (10f - Vector2.Distance(__instance.Hotspot.transform.localPosition, __instance.Phone.transform.localPosition)) * num / __instance.BeepPeriod;

            if (__instance.beepTimer >= 1f)
            {
                __instance.beepTimer = 0f;
                SoundManager.Instance.PlaySoundImmediate(__instance.nearSound, false);
            }
        }

        if (__instance.phoneGrabbed)
        {
            Vector3 vector = __instance.Phone.transform.position;
            float z = vector.z;
            vector = PassiveButtonManager.Instance.controller.Touches[0].Position;
            vector.z = z;
            __instance.Phone.transform.position = vector;
        }

        __instance.gauge.Value = __instance.timer / 20f;

        if (__instance.timer >= 20f)
        {
            __instance.MyNormTask.NextStep();
            __instance.Close();
        }
    }
}
