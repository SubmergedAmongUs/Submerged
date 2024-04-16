using System;
using Assets.CoreScripts;
using UnityEngine;

public class StoreArmsTaskConsole : global::Console
{
	public AudioClip useSound;

	public Sprite[] Images;

	public int usesPerStep = 2;

	private int timesUsed;

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
		int num = this.timesUsed + 1;
		this.timesUsed = num;
		if (num % this.usesPerStep == 0)
		{
			task.NextStep();
		}
		int num2 = this.timesUsed % this.Images.Length;
		this.Image.sprite = this.Images[num2];
	}
}
