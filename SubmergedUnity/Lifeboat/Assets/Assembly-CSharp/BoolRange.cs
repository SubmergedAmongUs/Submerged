using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoolRange
{
	public static bool Next(float p = 0.5f)
	{
		return Random.value <= p;
	}
}
