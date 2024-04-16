using System;
using System.Linq;
using System.Text;
using UnityEngine;

public class ReactorTask : SabotageTask
{
	private bool isComplete;

	private ICriticalSabotage reactor;

	private bool even;

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
		base.SetupArrows();
	}

	public void Awake()
	{
		this.reactor = (ICriticalSabotage)ShipStatus.Instance.Systems[this.StartAt];
		DestroyableSingleton<HudManager>.Instance.StartReactorFlash();
		ReactorShipRoom reactorShipRoom = ShipStatus.Instance.AllRooms.FirstOrDefault((PlainShipRoom r) => r.RoomId == this.StartAt) as ReactorShipRoom;
		if (reactorShipRoom != null)
		{
			reactorShipRoom.StartMeltdown();
		}
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
		}
	}

	public override bool ValidConsole(global::Console console)
	{
		return console.TaskTypes.Contains(this.TaskType);
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
		sb.Append(color.ToTextColor());
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(this.TaskType));
		sb.Append(" ");
		sb.Append((int)this.reactor.Countdown);
		sb.AppendLine(string.Format(" ({0}/{1})</color>", this.reactor.UserCount, 2));
		for (int i = 0; i < this.Arrows.Length; i++)
		{
			this.Arrows[i].image.color = color;
		}
	}
}
