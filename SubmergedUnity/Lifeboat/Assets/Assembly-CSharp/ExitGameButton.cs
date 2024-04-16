using System;
using InnerNet;
using UnityEngine;

public class ExitGameButton : MonoBehaviour
{
	public void Start()
	{
		if (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void OnClick()
	{
		if (AmongUsClient.Instance)
		{
			AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
			return;
		}
		SceneChanger.ChangeScene("MainMenu");
	}
}
