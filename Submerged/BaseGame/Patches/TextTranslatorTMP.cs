using HarmonyLib;
using TMPro;

namespace Submerged.BaseGame.Patches;

[HarmonyPatch]
public static class TextTranslatorTMPPatches
{
    [HarmonyPatch(typeof(TextTranslatorTMP), nameof(TextTranslatorTMP.ResetText))]
    [HarmonyPrefix]
    [BaseGameCode(LastChecked.v2024_3_5, "We are patching this with its own code to get rid of inlining that ruins our translation patches")]
    public static void ResetTextPatch(TextTranslatorTMP __instance, out bool __runOriginal)
    {
        __runOriginal = false;

        if (__instance.ResetOnlyWhenNoDefault) return;

        TextMeshPro component = __instance.GetComponent<TextMeshPro>();
        string text = DestroyableSingleton<TranslationController>.Instance.GetStringWithDefault(__instance.TargetText, __instance.defaultStr);

        if (__instance.ToUpper) text = text.ToUpperInvariant();

        if (component != null)
        {
            component.text = text;
            component.ForceMeshUpdate();
        }
        else
        {
            TextMeshProUGUI component2 = __instance.GetComponent<TextMeshProUGUI>();
            component2.text = text;
            component2.ForceMeshUpdate();
        }

        __instance.OnTranslate?.Invoke();
    }
}
