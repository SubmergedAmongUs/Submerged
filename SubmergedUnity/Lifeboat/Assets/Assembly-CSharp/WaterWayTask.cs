using System;
using System.Linq;

public class WaterWayTask : NormalPlayerTask
{
	public override void Initialize()
	{
		base.Initialize();
		this.Data = new byte[3];
	}

	public override bool ValidConsole(global::Console console)
	{
		return console.TaskTypes.Contains(this.TaskType) && this.Data[console.ConsoleId] < byte.MaxValue;
	}
}
