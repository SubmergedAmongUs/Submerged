using System;
using System.Collections;

public class AutoMultistageMinigame : Minigame
{
	public Minigame[] Stages;

	private Minigame stage;

	public override void Begin(PlayerTask task)
	{
		NormalPlayerTask normalPlayerTask = task as NormalPlayerTask;
		for (int i = 0; i < this.Stages.Length; i++)
		{
			this.Stages[i].gameObject.SetActive(i == normalPlayerTask.taskStep);
		}
		this.stage = this.Stages[normalPlayerTask.taskStep];
		this.stage.Console = base.Console;
		this.stage.Begin(task);
		Minigame.Instance = this;
		base.StartCoroutine(this.Run());
	}

	private IEnumerator Run()
	{
		while (this.stage)
		{
			yield return null;
		}
		this.Close();
		yield break;
	}

	public override void Close()
	{
		Minigame.Instance = null;
		if (this.stage)
		{
			this.stage.Close();
		}
		base.Close();
	}
}
