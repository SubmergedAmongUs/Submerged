using System;

[Serializable]
public class TaskSet
{
	public TaskTypes taskType;
	public IntRange taskStep = new IntRange(0, 0);
}
