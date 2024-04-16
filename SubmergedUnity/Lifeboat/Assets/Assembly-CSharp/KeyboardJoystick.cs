using System;
using UnityEngine;

public class KeyboardJoystick : MonoBehaviour, IVirtualJoystick
{
	private Vector2 del;

	public Vector2 Delta
	{
		get
		{
			return this.del;
		}
	}

	private void Update()
	{
		if (!PlayerControl.LocalPlayer)
		{
			return;
		}
		this.del.x = (this.del.y = 0f);
		if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			this.del.x = this.del.x + 1f;
		}
		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			this.del.x = this.del.x - 1f;
		}
		if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			this.del.y = this.del.y + 1f;
		}
		if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			this.del.y = this.del.y - 1f;
		}
		KeyboardJoystick.HandleHud();
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (Minigame.Instance)
			{
				Minigame.Instance.Close();
			}
			else if (DestroyableSingleton<HudManager>.InstanceExists && MapBehaviour.Instance && MapBehaviour.Instance.IsOpen)
			{
				MapBehaviour.Instance.Close();
			}
			else if (CustomPlayerMenu.Instance)
			{
				CustomPlayerMenu.Instance.Close(true);
			}
		}
		this.del.Normalize();
	}

	private static void HandleHud()
	{
		if (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			DestroyableSingleton<HudManager>.Instance.ReportButton.DoClick();
		}
		if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
		{
			DestroyableSingleton<HudManager>.Instance.UseButton.DoClick();
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			DestroyableSingleton<HudManager>.Instance.ShowMap(delegate(MapBehaviour m)
			{
				m.ShowNormalMap();
			});
		}
		if (PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.IsImpostor && Input.GetKeyDown(KeyCode.Q))
		{
			DestroyableSingleton<HudManager>.Instance.KillButton.PerformKill();
		}
	}
}
