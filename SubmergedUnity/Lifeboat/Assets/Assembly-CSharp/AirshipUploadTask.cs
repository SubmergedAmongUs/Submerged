using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AirshipUploadTask : NormalPlayerTask
{
	public ArrowBehaviour[] Arrows;

	public override bool ValidConsole(global::Console console)
	{
		return (console.Room == this.StartAt && console.ValidTasks.Any((TaskSet set) => this.TaskType == set.taskType && set.taskStep.Contains(this.taskStep))) || (this.taskStep == 1 && console.TaskTypes.Contains(this.TaskType));
	}

	protected override void FixedUpdate()
	{
		if (this.Arrows[0].isActiveAndEnabled && PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.arrowSuspended = true;
			this.Arrows.SetAllGameObjectsActive(false);
			return;
		}
		if (this.arrowSuspended && !PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.arrowSuspended = false;
			this.Arrows.SetAllGameObjectsActive(true);
		}
	}

	public override void UpdateArrow()
	{
		if (this.taskStep == 0 || this.IsComplete || !base.Owner.AmOwner)
		{
			this.Arrows.SetAllGameObjectsActive(false);
			return;
		}
		if (PlayerTask.PlayerHasTaskOfType<IHudOverrideTask>(PlayerControl.LocalPlayer))
		{
			this.arrowSuspended = true;
		}
		else
		{
			this.Arrows.SetAllGameObjectsActive(true);
		}
		List<Vector2> list = base.FindValidConsolesPositions();
		int num = 0;
		while (num < list.Count && num < this.Arrows.Length)
		{
			this.Arrows[num].target = list[num];
			num++;
		}
		this.LocationDirty = true;
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		if (this.taskStep > 0)
		{
			if (this.IsComplete)
			{
				sb.Append("<color=#00DD00FF>");
			}
			else
			{
				sb.Append("<color=#FFFF00FF>");
			}
		}
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((this.taskStep == 0) ? this.StartAt : SystemTypes.Outside));
		sb.Append(": ");
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((this.taskStep == 0) ? StringNames.DownloadData : StringNames.UploadData, Array.Empty<object>()));
		sb.Append(" (");
		sb.Append(this.taskStep);
		sb.Append("/");
		sb.Append(this.MaxStep);
		sb.AppendLine(") </color>");
	}
}
