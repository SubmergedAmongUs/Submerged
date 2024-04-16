using System;
using UnityEngine;

public class VentCleaningConsole : global::Console
{
	public AudioClip ImpostorDiscoveredSound;

	public override void Use()
	{
		bool flag;
		bool flag2;
		base.CanUse(PlayerControl.LocalPlayer.Data, out flag, out flag2);
		if (!flag)
		{
			return;
		}
		VentilationSystem ventilationSystem = ShipStatus.Instance.Systems[SystemTypes.Ventilation] as VentilationSystem;
		if (!PlayerControl.LocalPlayer.Data.IsDead && ventilationSystem != null && ventilationSystem.IsImpostorInsideVent(this.ConsoleId))
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.ImpostorDiscoveredSound, false, 0.8f);
			}
			VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, this.ConsoleId);
			return;
		}
		base.Use();
	}
}
