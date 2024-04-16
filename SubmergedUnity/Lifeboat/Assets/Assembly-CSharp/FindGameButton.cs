using System;
using System.Collections;
using InnerNet;
using PowerTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FindGameButton : MonoBehaviour, IConnectButton
{
	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public void OnClick()
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
		if (!DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			return;
		}
		AmongUsClient.Instance.GameMode = GameModes.OnlineGame;
		AmongUsClient.Instance.MainMenuScene = "MMOnline";
		base.StartCoroutine(this.ConnectForFindGame());
	}

	private IEnumerator ConnectForFindGame()
	{
		AmongUsClient.Instance.SetEndpoint(DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress, DestroyableSingleton<ServerManager>.Instance.OnlineNetPort);
		AmongUsClient.Instance.OnlineScene = "OnlineGame";
		AmongUsClient.Instance.mode = MatchMakerModes.Client;
		yield return AmongUsClient.Instance.CoConnect();
		if (AmongUsClient.Instance.LastDisconnectReason != DisconnectReasons.ExitGame)
		{
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		}
		else
		{
			AmongUsClient.Instance.HostId = AmongUsClient.Instance.ClientId;
			SceneManager.LoadScene("FindAGame");
		}
		yield break;
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
