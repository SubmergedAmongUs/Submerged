using System;
using UnityEngine;

public class DivertPowerMetagame : Minigame
{
	public Minigame DistributePrefab;

	public Minigame ReceivePrefab;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		Minigame minigame;
		if (this.MyNormTask.taskStep == 0)
		{
			minigame = UnityEngine.Object.Instantiate<Minigame>(this.DistributePrefab, base.transform.parent);
		}
		else
		{
			minigame = UnityEngine.Object.Instantiate<Minigame>(this.ReceivePrefab, base.transform.parent);
		}
		minigame.Begin(task);
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
