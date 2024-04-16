using System;
using System.Linq;
using System.Text;

public class DivertPowerTask : NormalPlayerTask
{
	public SystemTypes TargetSystem;

	public override bool ValidConsole(global::Console console)
	{
		return (console.Room == this.TargetSystem && console.ValidTasks.Any((TaskSet set) => set.Contains(this))) || (this.taskStep == 0 && console.TaskTypes.Contains(this.TaskType));
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
		if (this.taskStep == 0)
		{
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(this.StartAt));
			sb.Append(": ");
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.DivertPowerTo, new object[]
			{
				DestroyableSingleton<TranslationController>.Instance.GetString(this.TargetSystem)
			}));
		}
		else
		{
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(this.TargetSystem));
			sb.Append(": ");
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.AcceptDivertedPower, Array.Empty<object>()));
		}
		sb.Append(" (");
		sb.Append(this.taskStep);
		sb.Append("/");
		sb.Append(this.MaxStep);
		sb.AppendLine(")");
		if (this.taskStep > 0)
		{
			sb.Append("</color>");
		}
	}
}
