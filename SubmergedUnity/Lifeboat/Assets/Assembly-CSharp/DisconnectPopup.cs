using System;
using InnerNet;
using TMPro;

public class DisconnectPopup : DestroyableSingleton<DisconnectPopup>
{
	public TextMeshPro TextArea;

	public void Start()
	{
		if (DestroyableSingleton<DisconnectPopup>.Instance == this)
		{
			this.Show();
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(true);
		this.DoShow();
	}

	private void DoShow()
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		if (!AmongUsClient.Instance)
		{
			base.gameObject.SetActive(false);
			return;
		}
		string text = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
		DisconnectReasons lastDisconnectReason = AmongUsClient.Instance.LastDisconnectReason;
		switch (lastDisconnectReason)
		{
		case DisconnectReasons.ExitGame:
		case DisconnectReasons.Destroy:
			base.gameObject.SetActive(false);
			return;
		case DisconnectReasons.GameFull:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorFullGame, Array.Empty<object>());
			return;
		case DisconnectReasons.GameStarted:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorStartedGame, Array.Empty<object>());
			return;
		case DisconnectReasons.GameNotFound:
		case DisconnectReasons.IncorrectGame:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorNotFoundGame, Array.Empty<object>());
			return;
		case (DisconnectReasons)4:
		case DisconnectReasons.Custom:
		case (DisconnectReasons)12:
		case (DisconnectReasons)13:
		case (DisconnectReasons)14:
		case (DisconnectReasons)15:
			break;
		case DisconnectReasons.IncorrectVersion:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorIncorrectVersion, Array.Empty<object>());
			return;
		case DisconnectReasons.Banned:
			if (text != null)
			{
				this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorBanned, new object[]
				{
					text
				});
				return;
			}
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorBannedNoCode, Array.Empty<object>());
			return;
		case DisconnectReasons.Kicked:
			if (text != null)
			{
				this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorKicked, new object[]
				{
					text
				});
				return;
			}
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorKickedNoCode, Array.Empty<object>());
			return;
		case DisconnectReasons.InvalidName:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorInvalidName, new object[]
			{
				SaveManager.PlayerName
			});
			return;
		case DisconnectReasons.Hacking:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorHacking, Array.Empty<object>());
			return;
		case DisconnectReasons.NotAuthorized:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorNotAuthenticated, Array.Empty<object>());
			return;
		case DisconnectReasons.Error:
			if (AmongUsClient.Instance.GameMode == GameModes.OnlineGame)
			{
				this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorGenericOnlineDisconnect, Array.Empty<object>());
				return;
			}
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorGenericLocalDisconnect, Array.Empty<object>());
			return;
		case DisconnectReasons.ServerRequest:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorInactivity, Array.Empty<object>());
			return;
		case DisconnectReasons.ServerFull:
			this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorServerOverload, Array.Empty<object>());
			return;
		default:
			if (lastDisconnectReason == DisconnectReasons.IntentionalLeaving)
			{
				this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorIntentionalLeaving, new object[]
				{
					StatsManager.Instance.BanMinutesLeft
				});
				return;
			}
			if (lastDisconnectReason == DisconnectReasons.FocusLost)
			{
				this.TextArea.text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorFocusLost, Array.Empty<object>());
				return;
			}
			break;
		}
		this.TextArea.text = (string.IsNullOrWhiteSpace(AmongUsClient.Instance.LastCustomDisconnect) ? DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.ErrorUnknown, Array.Empty<object>()) : AmongUsClient.Instance.LastCustomDisconnect);
	}

	public void ShowCustom(string message)
	{
		base.gameObject.SetActive(true);
		this.TextArea.text = message;
	}

	public void Close()
	{
		base.gameObject.SetActive(false);
	}
}
