using System.Text;
using HarmonyLib;
using Submerged.BaseGame;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Loading;
using Submerged.Localization.Strings;
using TMPro;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class StartGameErrorPatches
{
    private static string _lastErrorMessage;

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void UpdateCustomLobbyInfoPatch(GameStartManager __instance)
    {
        if (!GameManager.Instance || GameManager.Instance.LogicOptions == null) return;

        if (!AmongUsClient.Instance.AmHost || GameManager.Instance.LogicOptions.MapId != (byte) CustomMapTypes.Submerged)
        {
            UpdateErrorMessage(__instance);
            return;
        }

        StringBuilder sb = null;

        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls.GetFastEnumerator())
        {
            CustomPlayerData playerData = playerControl.gameObject.EnsureComponent<CustomPlayerData>();
            if (playerData.HasMap) continue;

            if (sb == null)
            {
                sb = new StringBuilder();
                sb.Append(General.Error_PlayersMissingSubmerged);
                sb.Append(' ');
            }
            else
            {
                sb.Append(", ");
            }

            sb.Append(playerControl.name);
        }

        UpdateErrorMessage(__instance, sb?.ToString());
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    [HarmonyPrefix]
    public static void PreventCheckBypass(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (__instance.startState != GameStartManager.StartingStates.Countdown) return;
        if (_lastErrorMessage.IsNullOrWhiteSpace()) return; // Take care, as updating the error message with text but without wasError will break this

        __instance.ResetStartState();
    }

    private static void UpdateErrorMessage(GameStartManager gameStartManager, string text = "")
    {
        if (_lastErrorMessage == text) return;
        _lastErrorMessage = text;

        if (text.IsNullOrWhiteSpace())
        {
            UpdateStartButtonBasedOnPlayerCount(gameStartManager);
            return;
        }

        gameStartManager.StartButton.SetButtonEnableState(false);
        if (gameStartManager.StartButtonGlyph != null) gameStartManager.StartButtonGlyph.SetColor(Palette.DisabledClear);
        gameStartManager.StartButton.buttonText.enableWordWrapping = true;
        gameStartManager.StartButton.buttonText.alignment = TextAlignmentOptions.Center;
        gameStartManager.StartButton.ChangeButtonText(text);
    }

    [BaseGameCode(LastChecked.v2025_5_20, "This code is taken from GameStartManager.Update")]
    private static void UpdateStartButtonBasedOnPlayerCount(GameStartManager gameStartManager)
    {
        gameStartManager.StartButton.SetButtonEnableState(gameStartManager.LastPlayerCount >= gameStartManager.MinPlayers);
        ActionMapGlyphDisplay startButtonGlyph = gameStartManager.StartButtonGlyph;
        if (startButtonGlyph != null)
        {
            startButtonGlyph.SetColor(gameStartManager.LastPlayerCount >= gameStartManager.MinPlayers ? Palette.EnabledColor : Palette.DisabledClear);
        }
        if (gameStartManager.LastPlayerCount >= gameStartManager.MinPlayers)
        {
            gameStartManager.StartButton.ChangeButtonText(TranslationController.Instance.GetString(StringNames.StartLabel, []));
            gameStartManager.GameStartTextClient.text = TranslationController.Instance.GetString(StringNames.WaitingForHost, []);
        }
        else
        {
            gameStartManager.StartButton.ChangeButtonText(TranslationController.Instance.GetString(StringNames.WaitingForPlayers, []));
            gameStartManager.GameStartTextClient.text = TranslationController.Instance.GetString(StringNames.WaitingForPlayers, []);
        }
    }
}
