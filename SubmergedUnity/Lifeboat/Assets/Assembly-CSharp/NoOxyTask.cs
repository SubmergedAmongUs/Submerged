using System;
using System.Linq;
using System.Text;
using UnityEngine;

public class NoOxyTask : SabotageTask
{
	private bool isComplete;

	private LifeSuppSystemType reactor;

	private bool even;

	public int targetNumber;

	public override int TaskStep
	{
		get
		{
			return this.reactor.UserCount;
		}
	}

	public override bool IsComplete
	{
		get
		{
			return this.isComplete;
		}
	}

	public override void Initialize()
	{
		this.targetNumber = IntRange.Next(0, 99999);
		ShipStatus instance = ShipStatus.Instance;
		this.reactor = (LifeSuppSystemType)instance.Systems[SystemTypes.LifeSupp];
		DestroyableSingleton<HudManager>.Instance.StartOxyFlash();
		base.SetupArrows();
	}

	private void FixedUpdate()
	{
		if (this.isComplete)
		{
			return;
		}
		if (!this.reactor.IsActive)
		{
			this.Complete();
			return;
		}
		for (int i = 0; i < this.Arrows.Length; i++)
		{
			this.Arrows[i].gameObject.SetActive(!this.reactor.GetConsoleComplete(i));
		}
	}

	public override bool ValidConsole(global::Console console)
	{
		return !this.reactor.GetConsoleComplete(console.ConsoleId) && console.TaskTypes.Contains(TaskTypes.RestoreOxy);
	}

	public override void OnRemove()
	{
	}

	public override void Complete()
	{
		this.isComplete = true;
		PlayerControl.LocalPlayer.RemoveTask(this);
		if (this.didContribute)
		{
			StatsManager instance = StatsManager.Instance;
			uint sabsFixed = instance.SabsFixed;
			instance.SabsFixed = sabsFixed + 1U;
		}
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		this.even = !this.even;
		Color color = this.even ? Color.yellow : Color.red;
		if (this.reactor != null)
		{
			sb.Append(color.ToTextColor());
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskTypes.RestoreOxy));
			sb.Append(" ");
			sb.Append(Mathf.CeilToInt(this.reactor.Countdown));
			sb.AppendLine(string.Format(" ({0}/{1})</color>", this.reactor.UserCount, 2));
		}
		else
		{
			sb.AppendLine(color.ToTextColor() + "Oxygen depleting</color>");
		}
		for (int i = 0; i < this.Arrows.Length; i++)
		{
			try
			{
				this.Arrows[i].image.color = color;
			}
			catch
			{
			}
		}
	}
}
