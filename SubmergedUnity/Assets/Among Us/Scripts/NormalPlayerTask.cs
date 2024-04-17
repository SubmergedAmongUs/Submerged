using System;

public class NormalPlayerTask : PlayerTask
{
	[Obsolete("", true)]
	public void NextStep()
	{
	}

	public int taskStep;
	public int MaxStep;
	public bool ShowTaskStep = true;
	public bool ShowTaskTimer;
	public TimerState TimerStarted;
	public float TaskTimer;
	public byte[] Data;
	public ArrowBehaviour Arrow;

	public enum TimerState
	{
		NotStarted,
		Started,
		Finished
	}
}
