using System;
using InnerNet;
using UnityEngine;

public class WaitForHostPopup : DestroyableSingleton<WaitForHostPopup>
{
	public GameObject Content;

	[Header("Console Controller Navigation")]
	public UiElement DefaultButtonSelected;

	public void Show()
	{
		if (AmongUsClient.Instance && AmongUsClient.Instance.ClientId > 0)
		{
			this.Content.SetActive(true);
			ControllerManager.Instance.OpenOverlayMenu(base.name, null, this.DefaultButtonSelected);
		}
	}

	public void ExitGame()
	{
		AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
		this.Content.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}

	public void Hide()
	{
		this.Content.SetActive(false);
		ControllerManager.Instance.CloseOverlayMenu(base.name);
	}
}
