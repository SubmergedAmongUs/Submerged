using System;

[Serializable]
public class FloatRange
{
	public FloatRange(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public float min;
	public float max;
}
