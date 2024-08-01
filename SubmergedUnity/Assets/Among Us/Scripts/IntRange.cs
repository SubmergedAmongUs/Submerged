using System;

[Serializable]
public class IntRange
{
	public IntRange(int min, int max)
	{
		this.min = min;
		this.max = max;
	}

	public int min;
	public int max;
}
