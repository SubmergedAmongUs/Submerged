using System;
using System.Linq;
using System.Text;

public class TowelTask : NormalPlayerTask
{
	public override bool ValidConsole(global::Console console)
	{
		if (this.TaskType == TaskTypes.PickUpTowels && console.TaskTypes.Contains(TaskTypes.PickUpTowels))
		{
			if (this.Data.IndexOf((byte b) => (int)b == console.ConsoleId) != -1)
			{
				return true;
			}
			if (this.Data.All((byte b) => b == 250) && console.ConsoleId == 255)
			{
				return true;
			}
		}
		return false;
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		int num = this.Data.Count((byte b) => b == 250);
		bool flag = num > 0;
		if (flag)
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
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(this.StartAt));
		sb.Append(": ");
		sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(this.TaskType));
		if (num < this.Data.Length)
		{
			sb.Append(" (");
			sb.Append(num);
			sb.Append("/");
			sb.Append(this.Data.Length);
			sb.Append(")");
		}
		if (flag)
		{
			sb.Append("</color>");
		}
		sb.AppendLine();
	}
}
