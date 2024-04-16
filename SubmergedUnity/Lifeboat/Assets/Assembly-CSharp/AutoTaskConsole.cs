using System;
using Assets.CoreScripts;
using UnityEngine;

public class AutoTaskConsole : global::Console
{
	public AudioClip useSound;

	private PlayerTask FindTask(PlayerControl pc)
	{
		for (int i = 0; i < pc.myTasks.Count; i++)
		{
			PlayerTask playerTask = pc.myTasks[i];
			if (!playerTask.IsComplete && playerTask.ValidConsole(this))
			{
				return playerTask;
			}
		}
		return null;
	}

	public override void Use()
	{
		bool flag;
		bool flag2;
		base.CanUse(PlayerControl.LocalPlayer.Data, out flag, out flag2);
		if (!flag)
		{
			return;
		}
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		NormalPlayerTask normalPlayerTask = (NormalPlayerTask)this.FindTask(localPlayer);
		this.AfterUse(normalPlayerTask);
		DestroyableSingleton<Telemetry>.Instance.WriteUse(localPlayer.PlayerId, normalPlayerTask.TaskType, base.transform.position);
	}

	protected virtual void AfterUse(NormalPlayerTask task)
	{
		if (this.useSound && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.useSound, false, 1f);
		}
		task.NextStep();
		this.Image.color = Palette.HalfWhite;
	}
}
