using System;
using UnityEngine;

public class DiscordButton : MonoBehaviour
{
	public void LinkToDiscord()
	{
		DestroyableSingleton<DiscordManager>.Instance.LoginWithDiscord();
	}
}
