using System;
using System.Linq;
using System.Text;
using UnityEngine;

public class ElectricTask : SabotageTask
{
	private bool isComplete;

	private SwitchSystem system;

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
		this.system = (SwitchSystem)instance.Systems[SystemTypes.Electrical];
		base.SetupArrows();
	}

	private void FixedUpdate()
	{
		if (this.isComplete)
		{
			return;
		}
		if (this.system.ExpectedSwitches == this.system.ActualSwitches)
		{
			this.Complete();
		}
	}

	public override bool ValidConsole(global::Console console)
	{
		return console.TaskTypes.Contains(TaskTypes.FixLights);
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
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(TaskTypes.FixLights));
		sb.AppendLine(" (%" + ((int)(this.system.Level * 100f)).ToString() + ")</color>");
		for (int i = 0; i < this.Arrows.Length; i++)
		{
			if (this.Arrows[i].isActiveAndEnabled)
			{
				this.Arrows[i].image.color = color;
			}
		}
	}
}
