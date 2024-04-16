using System;
using InnerNet;
using UnityEngine;

public class MatchMaker : DestroyableSingleton<MatchMaker>
{
	public TextBoxTMP GameIdText;

	private MonoBehaviour Connecter;

	public void Start()
	{
		if (this.GameIdText && AmongUsClient.Instance)
		{
			this.GameIdText.SetText(GameCode.IntToGameName(AmongUsClient.Instance.GameId) ?? "", "");
		}
	}

	public bool Connecting(MonoBehaviour button)
	{
		if (!this.Connecter)
		{
			this.Connecter = button;
			((IConnectButton)this.Connecter).StartIcon();
			return true;
		}
		base.StartCoroutine(Effects.SwayX(this.Connecter.transform, 0.75f, 0.25f));
		return false;
	}

	public void NotConnecting()
	{
		if (this.Connecter)
		{
			((IConnectButton)this.Connecter).StopIcon();
			this.Connecter = null;
		}
	}
}
