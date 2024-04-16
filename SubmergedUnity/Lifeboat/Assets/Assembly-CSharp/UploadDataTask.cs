using System;
using System.Linq;
using System.Text;

public class UploadDataTask : NormalPlayerTask
{
	public SystemTypes EndAt = SystemTypes.Admin;

	public override bool ValidConsole(global::Console console)
	{
		return (console.Room == this.StartAt && console.ValidTasks.Any((TaskSet set) => this.TaskType == set.taskType && set.taskStep.Contains(this.taskStep))) || (this.taskStep == 1 && console.TaskTypes.Contains(this.TaskType));
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
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((this.taskStep == 0) ? this.StartAt : this.EndAt));
		sb.Append(": ");
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString((this.taskStep == 0) ? StringNames.DownloadData : StringNames.UploadData, Array.Empty<object>()));
		sb.Append(" (");
		sb.Append(this.taskStep);
		sb.Append("/");
		sb.Append(this.MaxStep);
		sb.AppendLine(") </color>");
	}
}
