using System;
using System.Collections;
using InnerNet;
using PowerTools;
using TMPro;
using UnityEngine;

public class JoinGameButton : MonoBehaviour, IConnectButton
{
	public AudioClip IntroMusic;

	public TextBoxTMP GameIdText;

	public TextMeshPro gameNameText;

	public float timeRecieved;

	public SpriteRenderer FillScreen;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public GameModes GameMode;

	public string netAddress;

	public void OnClick()
	{
		if (string.IsNullOrWhiteSpace(this.netAddress))
		{
			return;
		}
		if (this.GameMode == GameModes.OnlineGame && !DestroyableSingleton<AccountManager>.Instance.CanPlayOnline())
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.NotAuthorized;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
			return;
		}
		if (NameTextBehaviour.Instance && NameTextBehaviour.Instance.ShakeIfInvalid())
		{
			return;
		}
		if (StatsManager.Instance.AmBanned)
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.IntentionalLeaving;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
			return;
		}
		if (!DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			return;
		}
		AmongUsClient.Instance.GameMode = this.GameMode;
		if (this.GameMode == GameModes.OnlineGame)
		{
			AmongUsClient.Instance.SetEndpoint(DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress, 22023);
			AmongUsClient.Instance.MainMenuScene = "MMOnline";
			int num = GameCode.GameNameToInt(this.GameIdText.text);
			if (num == -1)
			{
				if (string.IsNullOrWhiteSpace(this.GameIdText.text))
				{
					TextTranslatorTMP component = this.gameNameText.GetComponent<TextTranslatorTMP>();
					if (component)
					{
						component.ResetText();
					}
				}
				base.StartCoroutine(Effects.SwayX(this.GameIdText.transform, 0.75f, 0.25f));
				DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
				return;
			}
			AmongUsClient.Instance.GameId = num;
		}
		else
		{
			AmongUsClient.Instance.SetEndpoint(this.netAddress, 22023);
			AmongUsClient.Instance.GameId = 32;
			AmongUsClient.Instance.GameMode = GameModes.LocalGame;
			AmongUsClient.Instance.MainMenuScene = "MatchMaking";
		}
		base.StartCoroutine(this.JoinGame());
	}

	private IEnumerator JoinGame()
	{
		if (this.FillScreen)
		{
			SoundManager.Instance.CrossFadeSound("MainBG", null, 0.5f, 1.5f);
			this.FillScreen.gameObject.SetActive(true);
			for (float time = 0f; time < 0.25f; time += Time.deltaTime)
			{
				this.FillScreen.color = Color.Lerp(Color.clear, Color.black, time / 0.25f);
				yield return null;
			}
			this.FillScreen.color = Color.black;
		}
		AmongUsClient.Instance.OnlineScene = "OnlineGame";
		AmongUsClient.Instance.Connect(MatchMakerModes.Client);
		yield return AmongUsClient.Instance.WaitForConnectionOrFail();
		if (AmongUsClient.Instance.mode == MatchMakerModes.None)
		{
			if (this.FillScreen)
			{
				SoundManager.Instance.CrossFadeSound("MainBG", this.IntroMusic, 0.5f, 1.5f);
				for (float time = 0f; time < 0.25f; time += Time.deltaTime)
				{
					this.FillScreen.color = Color.Lerp(Color.black, Color.clear, time / 0.25f);
					yield return null;
				}
				this.FillScreen.color = Color.clear;
			}
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		}
		yield break;
	}

	public void SetGameName(string[] gameNameParts)
	{
		int num = 15;
		this.gameNameText.text = string.Format("{0} ({1}/{2})", gameNameParts[0], gameNameParts[2], num);
	}

	public void StartIcon()
	{
		this.connectIcon.Play(this.connectClip, 1f);
	}

	public void StopIcon()
	{
		this.connectIcon.Stop();
		this.connectIcon.GetComponent<SpriteRenderer>().sprite = null;
	}
}
