using System.Text;
using HarmonyLib;
using Submerged.Enums;
using Submerged.Extensions;
using Submerged.Loading;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.UI.Patches;

[HarmonyPatch]
public static class StartGameErrorPatches
{
    private static TextMeshPro _lobbyInfoText;

    private static TextMeshPro LobbyInfoText
    {
        get
        {
            if (_lobbyInfoText) return _lobbyInfoText;

            GameObject playerCounterObj = GameObject.FindObjectOfType<GameStartManager>().PlayerCounter.gameObject;
            GameObject messageObj = UnityObject.Instantiate(playerCounterObj, playerCounterObj.transform.parent);
            TextMeshPro messageText = messageObj.GetComponent<TextMeshPro>();

            messageObj.name = "SubLobbyInfoText";
            messageObj.transform.GetChild(0).gameObject.SetActive(false);
            messageObj.transform.localPosition = new Vector3(0.02f, 0.5f, 0);
            messageText.fontSize = 2.3f;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.text = "";

            return _lobbyInfoText = messageText;
        }
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPostfix]
    public static void UpdateCustomLobbyInfoPatch(GameStartManager __instance)
    {
        if (!GameManager.Instance || GameManager.Instance.LogicOptions == null) return;

        if (!AmongUsClient.Instance.AmHost || GameManager.Instance.LogicOptions.MapId != (byte) CustomMapTypes.Submerged)
        {
            UpdateErrorMessage(__instance, false);
            return;
        }

        StringBuilder sb = null;

        foreach (PlayerControl playerControl in PlayerControl.AllPlayerControls.GetFastEnumerator())
        {
            CustomPlayerData playerData = playerControl.gameObject.EnsureComponent<CustomPlayerData>();
            if (playerData.HasMap) continue;

            if (sb == null)
            {
                sb = new StringBuilder("<color=#00AAFFFF>");
                sb.Append(General.Error_PlayersMissingSubmerged);
                sb.Append(' ');
            }
            else
            {
                sb.Append(", ");
            }

            sb.Append(playerControl.name);
        }

        UpdateErrorMessage(__instance, sb != null, sb?.ToString());
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.BeginGame))]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.ReallyBegin))]
    [HarmonyPriority(Priority.First)]
    [HarmonyPrefix]
    public static bool PreventBeginGamePatch(GameStartManager __instance)
    {
        if (LobbyInfoText.text.IsNullOrWhiteSpace()) return true;

        __instance.StartCoroutine(Effects.SwayX(LobbyInfoText.transform));

        return false;
    }

    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    [HarmonyPrefix]
    public static void PreventCheckBypass(GameStartManager __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if (__instance.startState != GameStartManager.StartingStates.Countdown) return;
        if (LobbyInfoText.text.IsNullOrWhiteSpace()) return; // Take care, as updating the error message with text but without wasError will break this

        __instance.ResetStartState();
    }

    private static void UpdateErrorMessage(GameStartManager gameStartManager, bool wasError, string text = "")
    {
        LobbyInfoText.autoSizeTextContainer = false;
        LobbyInfoText.text = text;
        LobbyInfoText.autoSizeTextContainer = true;

        Color startColor = wasError || gameStartManager.LastPlayerCount < gameStartManager.MinPlayers ? Palette.DisabledClear : Palette.EnabledColor;

        gameStartManager.StartButton.color = startColor;
        gameStartManager.startLabelText.color = startColor;
    }
}
