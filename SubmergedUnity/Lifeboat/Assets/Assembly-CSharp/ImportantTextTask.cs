using System;
using System.Text;

public class ImportantTextTask : PlayerTask
{
	public string Text;

	public override int TaskStep
	{
		get
		{
			return 0;
		}
	}

	public override bool IsComplete
	{
		get
		{
			return false;
		}
	}

	public override void Initialize()
	{
	}

	public override bool ValidConsole(global::Console console)
	{
		return false;
	}

	public override void Complete()
	{
	}

	public override void AppendTaskText(StringBuilder sb)
	{
		sb.AppendLine("<color=#FF0000FF>" + this.Text + "</color>");
	}
}
