using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class FloatRange
{
	public float min;

	public float max;

	public float Last { get; private set; }

	public float Width
	{
		get
		{
			return this.max - this.min;
		}
	}

	public FloatRange(float min, float max)
	{
		this.min = min;
		this.max = max;
	}

	public float ChangeRange(float y, float min, float max)
	{
		return Mathf.Lerp(min, max, (y - this.min) / this.Width);
	}

	public float Clamp(float value)
	{
		return Mathf.Clamp(value, this.min, this.max);
	}

	public bool Contains(float t)
	{
		return this.min <= t && this.max >= t;
	}

	public float CubicLerp(float v)
	{
		if (this.min >= this.max)
		{
			return this.min;
		}
		v = Mathf.Clamp(0f, 1f, v);
		return v * v * v * (this.max - this.min) + this.min;
	}

	public float EitherOr()
	{
		if (Random.value <= 0.5f)
		{
			return this.max;
		}
		return this.min;
	}

	public float LerpUnclamped(float v)
	{
		return Mathf.LerpUnclamped(this.min, this.max, v);
	}

	public float Lerp(float v)
	{
		return Mathf.Lerp(this.min, this.max, v);
	}

	public float ExpOutLerp(float v)
	{
		return this.Lerp(1f - Mathf.Pow(2f, -10f * v));
	}

	public static float ExpOutLerp(float v, float min, float max)
	{
		return Mathf.Lerp(min, max, 1f - Mathf.Pow(2f, -10f * v));
	}

	public static float Next(float min, float max)
	{
		return Random.Range(min, max);
	}

	public float Next()
	{
		return this.Last = Random.Range(this.min, this.max);
	}

	public float NextMinDistance(float center, float minDistance)
	{
		float num = Mathf.Abs(this.min - center);
		float num2 = Mathf.Abs(this.max - center);
		bool flag = num > minDistance;
		bool flag2 = num2 > minDistance;
		bool flag3;
		if (flag2 && flag2)
		{
			flag3 = BoolRange.Next(0.5f);
		}
		else if (!flag && !flag2)
		{
			flag3 = (num > num2);
			minDistance = num * 0.9f;
		}
		else
		{
			flag3 = flag;
		}
		if (flag3)
		{
			return this.Last = Random.Range(this.min, center - minDistance);
		}
		return this.Last = Random.Range(center + minDistance, this.max);
	}

	public IEnumerable<float> Range(int numStops)
	{
		float num;
		for (float i = 0f; i <= (float)numStops; i = num)
		{
			yield return Mathf.Lerp(this.min, this.max, i / (float)numStops);
			num = i + 1f;
		}
		yield break;
	}

	public IEnumerable<float> RandomRange(int numStops)
	{
		float num;
		for (float i = 0f; i <= (float)numStops; i = num)
		{
			yield return this.Next();
			num = i + 1f;
		}
		yield break;
	}

	internal float ReverseLerp(float t)
	{
		return Mathf.Clamp((t - this.min) / this.Width, 0f, 1f);
	}

	public static float ReverseLerp(float t, float min, float max)
	{
		float num = max - min;
		return Mathf.Clamp((t - min) / num, 0f, 1f);
	}

	public float SpreadToEdges(int idx, int stops)
	{
		if (stops == 1)
		{
			return this.min;
		}
		return Mathf.Lerp(this.min, this.max, (float)idx / ((float)stops - 1f));
	}

	public IEnumerable<float> SpreadToEdges(int stops)
	{
		return FloatRange.SpreadToEdges(this.min, this.max, stops);
	}

	public IEnumerable<float> SpreadEvenly(int stops)
	{
		return FloatRange.SpreadEvenly(this.min, this.max, stops);
	}

	public static float SpreadToEdges(float min, float max, int i, int stops)
	{
		if (stops == 1)
		{
			return min;
		}
		return Mathf.Lerp(min, max, (float)i / ((float)stops - 1f));
	}

	public static IEnumerable<float> SpreadToEdges(float min, float max, int stops)
	{
		if (stops == 1)
		{
			yield break;
		}
		int num;
		for (int i = 0; i < stops; i = num)
		{
			yield return Mathf.Lerp(min, max, (float)i / ((float)stops - 1f));
			num = i + 1;
		}
		yield break;
	}

	public static float SpreadEvenly(float min, float max, int i, int stops)
	{
		float num = (float)(i + 1) / ((float)stops + 1f);
		return Mathf.Lerp(min, max, num);
	}

	public static IEnumerable<float> SpreadEvenly(float min, float max, int stops)
	{
		float step = 1f / ((float)stops + 1f);
		for (float i = step; i < 1f; i += step)
		{
			yield return Mathf.Lerp(min, max, i);
		}
		yield break;
	}
}
