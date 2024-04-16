using System;
using UnityEngine;

public class ChainBehaviour : MonoBehaviour
{
	public FloatRange SwingRange = new FloatRange(0f, 30f);

	public float SwingPeriod = 2f;

	public float swingTime;

	private Vector3 vec;

	public void Awake()
	{
		this.swingTime = FloatRange.Next(0f, this.SwingPeriod);
		this.vec.z = this.SwingRange.Lerp(Mathf.Sin(this.swingTime));
		base.transform.eulerAngles = this.vec;
	}

	public void Update()
	{
		this.swingTime += Time.deltaTime / this.SwingPeriod;
		this.vec.z = this.SwingRange.Lerp(Mathf.Sin(this.swingTime * 3.1415927f) / 2f + 0.5f);
		base.transform.eulerAngles = this.vec;
	}
}
