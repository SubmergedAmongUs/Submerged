using System;
using TMPro;
using UnityEngine;

public class PingTracker : MonoBehaviour
{
	public TextMeshPro text;

	private void Update()
	{
		if (AmongUsClient.Instance)
		{
			if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
			{
				base.gameObject.SetActive(false);
			}
			this.text.text = string.Format("Ping: {0} ms", AmongUsClient.Instance.Ping);
		}
	}
}
