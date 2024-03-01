using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Submerged.Extensions;

namespace Submerged.BaseGame.Patches;

[HarmonyPatch]
public static class ExileControllerPatches
{
    [UsedImplicitly]
    public static void DisablePrefixWarningForHarmonyId(string id)
    {
        _allowedPrefixPatches.Add(id);
    }

    private static readonly SCG.HashSet<string> _allowedPrefixPatches = [SubmergedPlugin.Id];

    public static void ExileController_Begin(ExileController self, GameData.PlayerInfo exiled, bool tie)
    {
        if (self.specialInputHandler != null)
        {
            self.specialInputHandler.disableVirtualCursor = true;
        }

        ExileController.Instance = self;
        ControllerManager.Instance.CloseAndResetAll();
        self.exiled = exiled;
        self.Text.gameObject.SetActive(false);
        self.Text.text = string.Empty;
        int num = GameData.Instance.AllPlayers.Count(p => p.Role.IsImpostor && !p.IsDead && !p.Disconnected);

        if (exiled != null)
        {
            int num2 = GameData.Instance.AllPlayers.Count(p => p.Role.IsImpostor);

            if (!GameManager.Instance.LogicOptions.GetConfirmImpostor())
            {
                self.completeString = TranslationController.Instance.GetString(StringNames.ExileTextNonConfirm, exiled.PlayerName);
            }
            else if (exiled.Role.IsImpostor)
            {
                if (num2 > 1)
                {
                    self.completeString = TranslationController.Instance.GetString(StringNames.ExileTextPP, exiled.PlayerName);
                }
                else
                {
                    self.completeString = TranslationController.Instance.GetString(StringNames.ExileTextSP, exiled.PlayerName);
                }
            }
            else if (num2 > 1)
            {
                self.completeString = TranslationController.Instance.GetString(StringNames.ExileTextPN, exiled.PlayerName);
            }
            else
            {
                self.completeString = TranslationController.Instance.GetString(StringNames.ExileTextSN, exiled.PlayerName);
            }

            self.Player.UpdateFromEitherPlayerDataOrCache(exiled,
                PlayerOutfitType.Default,
                PlayerMaterial.MaskType.Exile,
                false,
                new Action(() =>
                {
                    SkinViewData skin = ShipStatus.Instance.CosmeticsCache.GetSkin(exiled.Outfits[PlayerOutfitType.Default].SkinId);

                    if (self.useIdleAnim)
                    {
                        self.Player.FixSkinSprite(skin.IdleFrame);
                        return;
                    }

                    self.Player.FixSkinSprite(skin.EjectFrame);
                }));
            self.Player.ToggleName(false);

            if (!self.useIdleAnim)
            {
                self.Player.SetCustomHatPosition(self.exileHatPosition);
                self.Player.SetCustomVisorPosition(self.exileVisorPosition);
            }

            if (exiled.Role.IsImpostor)
            {
                num--;
            }
        }
        else
        {
            if (tie)
            {
                self.completeString = TranslationController.Instance.GetString(StringNames.NoExileTie);
            }
            else
            {
                self.completeString = TranslationController.Instance.GetString(StringNames.NoExileSkip);
            }

            self.Player.gameObject.SetActive(false);
        }

        if (num == 1)
        {
            self.ImpostorText.text = TranslationController.Instance.GetString(StringNames.ImpostorsRemainS, num);
        }
        else
        {
            self.ImpostorText.text = TranslationController.Instance.GetString(StringNames.ImpostorsRemainP, num);
        }

        self.StartCoroutine(self.Animate());
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPrefix]
    [BaseGameCode(LastChecked.v2023_10_24, "Sometimes the text doesn't show up. We have no idea why so instead we just patch the method with its own code to fix it :)")]
    public static bool BeginPatch(ExileController __instance, MethodInfo __originalMethod, GameData.PlayerInfo exiled, bool tie)
    {
        HarmonyLib.Patches patchInfo = Harmony.GetPatchInfo(__originalMethod);

        Patch prefix = patchInfo.Prefixes.FirstOrDefault(p => p.PatchMethod.ReturnType == typeof(bool) && !_allowedPrefixPatches.Contains(p.owner));
        if (prefix != null)
        {
            Fatal($"A prefix patch returning bool was detected on ExileController.Begin! Be aware that Submerged also patches this method with a prefix that returns false, so this might lead to issues. Harmony ID at fault: {prefix.owner}");
            Fatal("In order to ensure compatibility, Submerged will disable its own patch on this method, however this might lead to issues when displaying the Submerged exile cutscene!!!");
            Fatal("In order to fix this, when Submerged is installed please patch Submerged.BaseGame.Patches.ExileControllerPatches.ExileController_Begin instead of ExileController.Begin, with the same patch. (Note that ExileController is the first argument of the method, not the __instance)");
            Fatal($"If you know what you are doing and want to remove this warning, call Submerged.BaseGame.Patches.ExileControllerPatches.DisablePrefixWarningForHarmonyId(\"{prefix.owner}\");");
            return true;
        }

        ExileController_Begin(__instance, exiled, tie);

        return false;
    }

    private static int Count<T>(this ICG.List<T> list, Predicate<T> predicate) where T : CppObject
    {
        int count = 0;

        foreach (T item in list.GetFastEnumerator())
        {
            if (predicate(item)) count++;
        }

        return count;
    }
}
