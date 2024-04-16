using System;

[Serializable]
public class TaskSet
{
	public TaskTypes taskType;

	public IntRange taskStep = new IntRange(0, 0);

	public bool Contains(PlayerTask t)
	{
		return t.TaskType == this.taskType && this.taskStep.Contains(t.TaskStep);
	}
}
