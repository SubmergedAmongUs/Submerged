using System;
using System.Collections;
using InnerNet;
using PowerTools;
using TMPro;
using UnityEngine;

public class MatchMakerGameButton : PoolableBehavior, IConnectButton
{
	public TextMeshPro NameText;

	public TextMeshPro PlayerCountText;

	public TextMeshPro ImpostorCountText;

	public SpriteRenderer MapIcon;

	public Sprite[] MapIcons;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public GameListing myListing;

	public void OnClick()
	{
		if (!DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			return;
		}
		if (this.myListing.IP != 0U)
		{
			AmongUsClient.Instance.GameMode = GameModes.OnlineGame;
			AmongUsClient.Instance.OnlineScene = "OnlineGame";
			AmongUsClient.Instance.SetEndpoint(InnerNetClient.AddressToString(this.myListing.IP), this.myListing.Port);
			Debug.Log("Connecting to: " + InnerNetClient.AddressToString(this.myListing.IP) + " for " + GameCode.IntToGameName(this.myListing.GameId));
			AmongUsClient.Instance.GameId = this.myListing.GameId;
			AmongUsClient.Instance.Connect(MatchMakerModes.Client);
			base.StartCoroutine(this.ConnectForFindGame());
			return;
		}
		AmongUsClient.Instance.GameMode = GameModes.OnlineGame;
		AmongUsClient.Instance.OnlineScene = "OnlineGame";
		AmongUsClient.Instance.GameId = this.myListing.GameId;
		AmongUsClient.Instance.JoinGame();
		base.StartCoroutine(this.ConnectForFindGame());
	}

	private IEnumerator ConnectForFindGame()
	{
		yield return EndGameManager.WaitWithTimeout(() => AmongUsClient.Instance.ClientId >= 0 || AmongUsClient.Instance.LastDisconnectReason > DisconnectReasons.ExitGame);
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
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

	public void SetGame(GameListing gameListing)
	{
		this.myListing = gameListing;
		this.NameText.text = this.myListing.HostName;
		this.ImpostorCountText.text = this.myListing.NumImpostors.ToString();
		this.PlayerCountText.text = string.Format("{0}/{1}", this.myListing.PlayerCount, this.myListing.MaxPlayers);
		this.MapIcon.sprite = this.MapIcons[Mathf.Clamp((int)this.myListing.MapId, 0, this.MapIcons.Length - 1)];
	}
}
