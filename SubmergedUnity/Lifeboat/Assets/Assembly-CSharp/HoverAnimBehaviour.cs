using System;
using UnityEngine;

public class HoverAnimBehaviour : MonoBehaviour
{
	public FloatRange YMovement;

	public float Speed = 1f;

	public float Shift = 1f;

	private float offset;

	private void Start()
	{
		this.offset = FloatRange.Next(0f, 10f);
	}

	private void Update()
	{
		Vector3 localPosition = base.transform.localPosition;
		float v = Mathf.Sin((Time.time + this.offset) * this.Speed) / 2f + this.Shift;
		localPosition.y = this.YMovement.Lerp(v);
		base.transform.localPosition = localPosition;
	}
}
