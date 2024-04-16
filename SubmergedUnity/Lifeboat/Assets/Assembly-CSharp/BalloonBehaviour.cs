using System;
using UnityEngine;

public class BalloonBehaviour : MonoBehaviour
{
	public Vector2 Origin;

	public float PeriodX = 4f;

	public float PeriodY = 4f;

	public float MagnitudeX = 3f;

	public float MagnitudeY = 3f;

	public void Update()
	{
		base.transform.localPosition = this.Origin + new Vector2(Mathf.PerlinNoise(Time.time * this.PeriodX, 1f) * this.MagnitudeX, Mathf.Sin(Time.time * this.PeriodY) * this.MagnitudeY);
	}
}
