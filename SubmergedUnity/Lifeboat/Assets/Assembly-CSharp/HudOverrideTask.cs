using System;
using System.Linq;
using System.Text;
using UnityEngine;

public class HudOverrideTask : SabotageTask, IHudOverrideTask
{
	private bool isComplete;

	private HudOverrideSystemType system;

	private bool even;

	public override int TaskStep
	{
		get
		{
			if (!this.isComplete)
			{
				return 0;
			}
			return 1;
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
		ShipStatus instance = ShipStatus.Instance;
		this.system = (instance.Systems[SystemTypes.Comms] as HudOverrideSystemType);
		base.SetupArrows();
	}

	private void FixedUpdate()
	{
		if (this.isComplete)
		{
			return;
		}
		if (!this.system.IsActive)
		{
			this.Complete();
		}
	}

	public override bool ValidConsole(global::Console console)
	{
		return console.TaskTypes.Contains(TaskTypes.FixComms);
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
		sb.Append(color.ToTextColor());
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskTypes.FixComms));
		sb.Append("</color>");
		for (int i = 0; i < this.Arrows.Length; i++)
		{
			this.Arrows[i].image.color = color;
		}
	}
}
