using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct Vector2Range
{
	public Vector2 min;

	public Vector2 max;

	public float Width
	{
		get
		{
			return this.max.x - this.min.x;
		}
	}

	public float Height
	{
		get
		{
			return this.max.y - this.min.y;
		}
	}

	public Vector2Range(Vector2 min, Vector2 max)
	{
		this.min = min;
		this.max = max;
	}

	public void LerpUnclamped(ref Vector3 output, float t, float z)
	{
		output.Set(Mathf.LerpUnclamped(this.min.x, this.max.x, t), Mathf.LerpUnclamped(this.min.y, this.max.y, t), z);
	}

	public void Lerp(ref Vector3 output, float t, float z)
	{
		output.Set(Mathf.Lerp(this.min.x, this.max.x, t), Mathf.Lerp(this.min.y, this.max.y, t), z);
	}

	public Vector2 Next()
	{
		return new Vector2(Random.Range(this.min.x, this.max.x), Random.Range(this.min.y, this.max.y));
	}

	public static Vector2 NextEdge()
	{
		float num = 6.2831855f * Random.value;
		float num2 = Mathf.Cos(num);
		float num3 = Mathf.Sin(num);
		return new Vector2(num2, num3);
	}
}
