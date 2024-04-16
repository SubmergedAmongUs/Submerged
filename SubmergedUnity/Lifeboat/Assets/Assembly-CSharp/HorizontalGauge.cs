using System;
using UnityEngine;

public class HorizontalGauge : MonoBehaviour
{
	public float Value = 0.5f;

	public float MaxValue = 1f;

	public float maskScale = 1f;

	public SpriteMask Mask;

	private float lastValue = float.MinValue;

	public void Update()
	{
		if (this.MaxValue != 0f && this.lastValue != this.Value)
		{
			this.lastValue = this.Value;
			float num = this.lastValue / this.MaxValue * this.maskScale;
			this.Mask.transform.localScale = new Vector3(num, 1f, 1f);
			this.Mask.transform.localPosition = new Vector3(-this.Mask.sprite.bounds.size.x * (this.maskScale - num) / 2f, 0f, 0f);
		}
	}
}
