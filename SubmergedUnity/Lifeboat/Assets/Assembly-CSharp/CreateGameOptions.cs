using System;
using System.Collections;
using System.Collections.Generic;
using InnerNet;
using PowerTools;
using UnityEngine;

public class CreateGameOptions : MonoBehaviour, IConnectButton
{
	public AudioClip IntroMusic;

	public GameObject Content;

	public SpriteRenderer Foreground;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	[Header("Console Controller Navigation")]
	public UiElement BackButton;

	public UiElement DefaultButtonSelected;

	public List<UiElement> ControllerSelectable;

	public PassiveButton RegionButton;

	public void Show()
	{
		if (!DestroyableSingleton<AccountManager>.Instance.CanPlayOnline())
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.NotAuthorized;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
			return;
		}
		if (StatsManager.Instance.AmBanned)
		{
			AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.IntentionalLeaving;
			DestroyableSingleton<DisconnectPopup>.Instance.Show();
			return;
		}
		base.gameObject.SetActive(true);
		this.Content.SetActive(false);
		base.StartCoroutine(this.CoShow());
	}

	private IEnumerator CoShow()
	{
		this.Foreground.gameObject.SetActive(true);
		yield return Effects.ColorFade(this.Foreground, Color.clear, Color.black, 0.1f);
		this.Content.SetActive(true);
		this.RegionButton.gameObject.SetActive(false);
		yield return null;
		ControllerManager.Instance.OpenOverlayMenu(base.name, this.BackButton, this.DefaultButtonSelected, this.ControllerSelectable, true);
		yield return Effects.ColorFade(this.Foreground, Color.black, Color.clear, 0.1f);
		this.Foreground.gameObject.SetActive(false);
		yield break;
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

	public void Hide()
	{
		base.StartCoroutine(this.CoHide());
	}

	private IEnumerator CoHide()
	{
		this.Foreground.gameObject.SetActive(true);
		yield return Effects.ColorFade(this.Foreground, Color.clear, Color.black, 0.1f);
		this.Content.SetActive(false);
		yield return Effects.ColorFade(this.Foreground, Color.black, Color.clear, 0.1f);
		this.Foreground.gameObject.SetActive(false);
		base.gameObject.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(base.name);
		this.RegionButton.gameObject.SetActive(true);
		yield break;
	}

	public void Confirm()
	{
		if (!DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			return;
		}
		base.StartCoroutine(this.CoStartGame());
	}

	private IEnumerator CoStartGame()
	{
		SoundManager.Instance.CrossFadeSound("MainBG", null, 0.5f, 1.5f);
		this.Foreground.gameObject.SetActive(true);
		yield return Effects.ColorFade(this.Foreground, Color.clear, Color.black, 0.2f);
		AmongUsClient.Instance.GameMode = GameModes.OnlineGame;
		AmongUsClient.Instance.SetEndpoint(DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress, DestroyableSingleton<ServerManager>.Instance.OnlineNetPort);
		AmongUsClient.Instance.MainMenuScene = "MMOnline";
		AmongUsClient.Instance.OnlineScene = "OnlineGame";
		AmongUsClient.Instance.Connect(MatchMakerModes.HostAndClient);
		yield return AmongUsClient.Instance.WaitForConnectionOrFail();
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		yield break;
	}
}
