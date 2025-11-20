using System.Linq;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Map;
using Submerged.Resources;
using UnityEngine;

namespace Submerged.HudMap.Patches;

[HarmonyPatch]
public static class AdminPatches
{
    [HarmonyPatch(typeof(UseButton), nameof(UseButton.Awake))]
    [HarmonyPrefix]
    public static void AddAdminUseButtonPatch(UseButton __instance)
    {
        if (__instance.UseSettings.Any(x => x.ButtonType == CustomImageNames.SubmergedAdminButton))
        {
            return;
        }

        UseButtonSettings submergedAdminButton = ScriptableObject.CreateInstance<UseButtonSettings>();
        submergedAdminButton.ButtonType = CustomImageNames.SubmergedAdminButton;
        submergedAdminButton.Image = ResourceManager.spriteCache["AdminButton"];
        submergedAdminButton.Text = StringNames.Admin;
        submergedAdminButton.FontMaterial = __instance.UseSettings[0].FontMaterial;
        __instance.UseSettings = __instance.UseSettings.AddItem(submergedAdminButton).ToArray();
    }
}
