using System;
using System.Collections;
using InnerNet;
using PowerTools;
using UnityEngine;

public class HostGameButton : MonoBehaviour, IConnectButton
{
	public AudioClip IntroMusic;

	public string targetScene;

	public SpriteRenderer FillScreen;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public GameModes GameMode;

	public void Start()
	{
		if (DestroyableSingleton<MatchMaker>.InstanceExists)
		{
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		}
	}

	public void OnClick()
	{
		if (this.GameMode == GameModes.FreePlay)
		{
			if (!NameTextBehaviour.IsValidName(SaveManager.PlayerName))
			{
				SaveManager.PlayerName = "";
			}
		}
		else
		{
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
		}
		base.StartCoroutine(this.CoStartGame());
	}

	public void StartIcon()
	{
		if (!this.connectIcon)
		{
			return;
		}
		this.connectIcon.Play(this.connectClip, 1f);
	}

	public void StopIcon()
	{
		if (!this.connectIcon)
		{
			return;
		}
		this.connectIcon.Stop();
		this.connectIcon.GetComponent<SpriteRenderer>().sprite = null;
	}

	private IEnumerator CoStartGame()
	{
		try
		{
			SoundManager.Instance.StopAllSound();
			AmongUsClient.Instance.GameMode = this.GameMode;
			switch (this.GameMode)
			{
			case GameModes.LocalGame:
				DestroyableSingleton<InnerNetServer>.Instance.StartAsServer();
				AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023);
				AmongUsClient.Instance.MainMenuScene = "MatchMaking";
				break;
			case GameModes.OnlineGame:
				AmongUsClient.Instance.SetEndpoint(DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress, 22023);
				AmongUsClient.Instance.MainMenuScene = "MMOnline";
				break;
			case GameModes.FreePlay:
				DestroyableSingleton<InnerNetServer>.Instance.StartAsLocalServer();
				AmongUsClient.Instance.SetEndpoint("127.0.0.1", 22023);
				AmongUsClient.Instance.MainMenuScene = "MainMenu";
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("HostGameButton::CoStartGame: Exception:");
			Debug.LogException(ex, this);
			DestroyableSingleton<DisconnectPopup>.Instance.ShowCustom(ex.Message);
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
			yield break;
		}
		yield return new WaitForSeconds(0.1f);
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
		AmongUsClient.Instance.OnlineScene = this.targetScene;
		AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient);
		yield return AmongUsClient.Instance.WaitForConnectionOrFail();
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		if (AmongUsClient.Instance.mode == MatchMakerModes.None && this.FillScreen)
		{
			SoundManager.Instance.CrossFadeSound("MainBG", this.IntroMusic, 0.5f, 1.5f);
			for (float time = 0f; time < 0.25f; time += Time.deltaTime)
			{
				this.FillScreen.color = Color.Lerp(Color.black, Color.clear, time / 0.25f);
				yield return null;
			}
			this.FillScreen.color = Color.clear;
		}
		yield break;
	}
}
