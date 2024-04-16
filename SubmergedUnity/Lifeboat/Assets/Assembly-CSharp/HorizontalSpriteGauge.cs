using System;
using UnityEngine;

public class HorizontalSpriteGauge : MonoBehaviour
{
	public float Value = 0.5f;

	public float MaxValue = 1f;

	public float maskScale = 1f;

	public SpriteRenderer Mask;

	private float lastValue = float.MinValue;

	public void Update()
	{
		if (this.MaxValue != 0f && this.lastValue != this.Value)
		{
			this.lastValue = this.Value;
			float num = this.lastValue / this.MaxValue * this.maskScale;
			Vector3 localScale = this.Mask.transform.localScale;
			localScale.x = num;
			this.Mask.transform.localScale = localScale;
			Vector3 localPosition = this.Mask.transform.localPosition;
			localPosition.x = -this.Mask.sprite.bounds.size.x * (this.maskScale - num) / 2f;
			this.Mask.transform.localPosition = localPosition;
		}
	}
}
