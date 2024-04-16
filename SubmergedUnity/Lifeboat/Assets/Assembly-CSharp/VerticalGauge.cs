using System;
using UnityEngine;

public class VerticalGauge : MonoBehaviour
{
	public float value = 0.5f;

	public float MaxValue = 1f;

	public float maskScale = 1f;

	public SpriteMask Mask;

	private float lastValue = float.MinValue;

	public void Update()
	{
		if (this.lastValue != this.value)
		{
			this.lastValue = this.value;
			float num = Mathf.Clamp(this.lastValue / this.MaxValue, 0f, 1f) * this.maskScale;
			Vector3 localScale = this.Mask.transform.localScale;
			localScale.y = num;
			this.Mask.transform.localScale = localScale;
			this.Mask.transform.localPosition = new Vector3(0f, -this.Mask.sprite.bounds.size.y * (this.maskScale - num) / 2f, 0f);
		}
	}
}
