using System;
using System.Linq;
using System.Text;

public class WeatherNodeTask : NormalPlayerTask
{
	public int NodeId;

	public Minigame Stage2Prefab;

	public override bool ValidConsole(global::Console console)
	{
		return (this.taskStep == 0 && console.ConsoleId == this.NodeId && console.TaskTypes.Contains(this.TaskType)) || console.ValidTasks.Any((TaskSet t) => t.Contains(this));
	}

	public override Minigame GetMinigamePrefab()
	{
		if (this.taskStep == 0)
		{
			return this.MinigamePrefab;
		}
		return this.Stage2Prefab;
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
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(WeatherSwitchGame.ControlNames[this.NodeId], Array.Empty<object>()));
			sb.Append(": ");
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FixWeatherNode, Array.Empty<object>()));
		}
		else
		{
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(this.StartAt));
			sb.Append(": ");
			sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.FixWeatherNode, Array.Empty<object>()));
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
