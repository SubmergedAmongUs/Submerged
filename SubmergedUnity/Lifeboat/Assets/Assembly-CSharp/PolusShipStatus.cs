using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolusShipStatus : ShipStatus
{
	protected override void OnEnable()
	{
		if (this.Systems != null)
		{
			return;
		}
		this.Systems = new Dictionary<SystemTypes, ISystemType>(ShipStatus.SystemTypeComparer.Instance)
		{
			{
				SystemTypes.Electrical,
				new SwitchSystem()
			},
			{
				SystemTypes.MedBay,
				new MedScanSystem()
			}
		};
		Camera main = Camera.main;
		main.backgroundColor = this.CameraColor;
		FollowerCamera component = main.GetComponent<FollowerCamera>();
		DestroyableSingleton<HudManager>.Instance.ShadowQuad.material.SetInt("_Mask", 7);
		this.Systems.Add(SystemTypes.Doors, new DoorsSystemType());
		this.Systems.Add(SystemTypes.Comms, new HudOverrideSystemType());
		this.Systems.Add(SystemTypes.Security, new SecurityCameraSystemType());
		this.Systems.Add(SystemTypes.Ventilation, new VentilationSystem());
		this.Systems.Add(SystemTypes.Laboratory, new ReactorSystemType(60f, SystemTypes.Laboratory));
		if (component)
		{
			component.shakeAmount = 0f;
			component.shakePeriod = 0f;
		}
		this.Systems.Add(SystemTypes.Sabotage, new SabotageSystemType((from i in this.Systems.Values
		where i is IActivatable
		select i).Cast<IActivatable>().ToArray<IActivatable>()));
	}

	public override void SpawnPlayer(PlayerControl player, int numPlayers, bool initialSpawn)
	{
		if (initialSpawn)
		{
			base.SpawnPlayer(player, numPlayers, initialSpawn);
			return;
		}
		int num = Mathf.FloorToInt((float)numPlayers / 2f);
		int num2 = (int)(player.PlayerId % 15);
		Vector2 position;
		if (num2 < num)
		{
			position = this.MeetingSpawnCenter + Vector2.right * (float)num2 * 0.6f;
		}
		else
		{
			position = this.MeetingSpawnCenter2 + Vector2.right * (float)(num2 - num) * 0.6f;
		}
		player.NetTransform.SnapTo(position);
	}
}
