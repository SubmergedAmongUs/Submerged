using System;
using UnityEngine;

public class Rotater : MonoBehaviour
{
	public float DegreesPerSecond = 360f;

	private void Update()
	{
		base.transform.localEulerAngles = new Vector3(0f, 0f, Time.time * this.DegreesPerSecond);
	}
}
